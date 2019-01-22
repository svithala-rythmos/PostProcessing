using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocWorks.Common.SEDAEvents.Examples.Helper;
using DocWorks.Common.SEDAEvents.Implementation.Sedav2;
using DocWorks.Common.SEDAEvents.Implementation.Sedav2.Aggregation;
using DocWorks.Common.SEDAEvents.Implementation.Sedav2.Utilities;
using DocWorks.Common.SEDAEvents.MongoDb;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Xunit;

namespace DocWorks.Common.SEDAEvents.Examples.Examples
{
    [Collection("MongoRunner Collection")]
    public class ScatterGather
    {
        public static List<string> eventResults = new List<string>();
        public static TestInitialization testInitialization;
        private EventBusService ebs;
        
        public ScatterGather(Mongo2GoRunnerFixture fixture)
        {
            testInitialization = new TestInitialization(fixture.Runner.ConnectionString);
        }
        
        [Fact]
        public async Task BasicScatterGatherTest()
        {
            //Arrange
            int departmentCount = new Random().Next(3, 15);
            string opid = ObjectId.GenerateNewId().ToString();
            ebs = await TestSetupEbs.SetupAndConfigure(x=>
            {
                x.AddSingleton(typeof(IMongoClientFactory),
                    testInitialization.mongoDb);
            }, false, typeof(DepartmentTeamNamesReader), typeof(TeamNameReader), typeof(GetTeamNamesFragmentHandler), typeof(GetDepartmentTeamNamesFragmentHandler), typeof(DepartmentAllTeamNameHandler));
            //Act
            await FireDepartmentEvents(departmentCount, opid);
            //Arrange
            Assert.True(DepartmentAllTeamNameHandler.amountOfTeamNames.All(x=>eventResults.Contains(x)));
        }
    
        async Task FireDepartmentEvents(int departmentCount, string opid)
        {
            for (int i = 0; i < departmentCount; i++)
            {
                await ebs.processMessage(new WantDepartmentTeamNames {EventIndex = i, departmentCount = departmentCount, operationId = opid}.ProduceOriginal());
            }
        }
    }

    public class WantDepartmentTeamNames : EventPayloadBase{
        public int EventIndex, departmentCount;
        public string operationId;
    }
    public class WantTeamNameReport : EventPayloadBase{
        public int EventIndex, departmentCount, departmentTeamCount;
        public string operationId ;
    }
    public class GotTeamName : EventPayloadBase{
        public int EventIndex, departmentCount, departmentTeamCount;
        public string operationId, teamName;
    }
    public class GotDepartmentTeamNames : EventPayloadBase
    {
        public int departmentCount;
        public string operationId;
        public string[] NamesOfTeams;
    }
    public class GotAllTeamNames : EventPayloadBase
    {
        public string[] AllTeamNames;
    }
    
    public class DepartmentTeamNamesReader : IHandleMessages<WantDepartmentTeamNames> //the amount of teams per department
    {   
        public EventBusService ebs;
        
        public DepartmentTeamNamesReader(EventBusService ebs)
        {
            this.ebs = ebs;
        }
        
        public async Task Handle(WantDepartmentTeamNames message)
        {
            int teamCount = new Random().Next(1, 15);
            for (int i = 0; i < teamCount; i++)
            {
                await ebs.processMessage(message.produceChildEvent(new WantTeamNameReport
                {
                    EventIndex = message.EventIndex,
                    departmentCount = message.departmentCount,
                    operationId = message.operationId,
                    departmentTeamCount = teamCount
                }));
            }
        }
    }
    public class TeamNameReader : IHandleMessages<WantTeamNameReport> // the names of the people in the team
    {        
        public EventBusService ebs;
        
        public TeamNameReader(EventBusService ebs)
        {
            this.ebs = ebs;
        }
        
        public async Task Handle(WantTeamNameReport message)
        {
            var name = message.EventIndex + "__" + ObjectId.GenerateNewId();
            ScatterGather.eventResults.Add(name);
            await ebs.processMessage(message.produceChildEvent(new GotTeamName
            {                    
                EventIndex = message.EventIndex,
                departmentCount = message.departmentCount,
                departmentTeamCount = message.departmentTeamCount,
                operationId = message.operationId,
                teamName = name
            }));
        }
    }
    public class GetTeamNamesFragmentHandler : IHandleMessages<GotTeamName>
    {
        private readonly DynamicAggregator<GotTeamName> _dma;
        private readonly EventBusService ebs;

        public GetTeamNamesFragmentHandler(DynamicAggregator<GotTeamName> dma, EventBusService ebs)
        {
            _dma = dma;
            this.ebs = ebs;
        }
        
        public async Task Handle(GotTeamName message)
        {
            await _dma.DynamicAggregate(message, message.EventIndex.ToString(), async aggData =>
                {
                    var gotTeamNameReports =  await aggData.GetDataFragmentsByType<GotTeamName>();
                    return gotTeamNameReports.Count() == gotTeamNameReports.First().departmentTeamCount;
                },
                async aggData =>
                {
                    var gotTeamNameReports =  await aggData.GetDataFragmentsByType<GotTeamName>();
                    await ebs.processMessage(message.produceChildEvent(new GotDepartmentTeamNames
                    {
                        NamesOfTeams = gotTeamNameReports.Select(x => x.teamName).ToArray(),
                        operationId = gotTeamNameReports.First().operationId,
                        departmentCount = gotTeamNameReports.First().departmentCount
                    }));
                });
        }
    }
    public class GetDepartmentTeamNamesFragmentHandler : IHandleMessages<GotDepartmentTeamNames>
    {
        private readonly DynamicCountingAggregator<GotDepartmentTeamNames> dma;
        private readonly EventBusService ebs;

        public GetDepartmentTeamNamesFragmentHandler(DynamicCountingAggregator<GotDepartmentTeamNames> dma, EventBusService ebs)
        {
            this.dma = dma;
            this.ebs = ebs;
        }
        public async Task Handle(GotDepartmentTeamNames message)
        {
            await dma.DynamicAggregate(message, message.operationId,
                async aggData =>
                {
                    var teamNames = await aggData.GetDataFragmentsByType<GotDepartmentTeamNames>();
                    List<string> aggregatoredListOfTeamNames = new List<string>();
                    teamNames.ToList().ForEach(arrayInsideOfIenumerable => arrayInsideOfIenumerable.NamesOfTeams.ToList().ForEach(y => aggregatoredListOfTeamNames.Add(y)));
                    await ebs.processMessage(message.produceChildEvent(new GotAllTeamNames
                    {
                        AllTeamNames = aggregatoredListOfTeamNames.ToArray()
                    }));
                }, message.departmentCount);
        }
    }
    public class DepartmentAllTeamNameHandler : IHandleMessages<GotAllTeamNames>
    {
        public static string[] amountOfTeamNames;
        
        public async Task Handle(GotAllTeamNames message)
        {
            amountOfTeamNames = message.AllTeamNames;
            await Task.CompletedTask;
        }
    }
}