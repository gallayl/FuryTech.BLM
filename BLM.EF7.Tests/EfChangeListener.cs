using System.Collections.Generic;
using System.Threading.Tasks;
using BLM.NetStandard.Interfaces;
using BLM.NetStandard.Tests;

namespace BLM.EF7.Tests
{
    public class EfChangeListener : Listener<MockEntity>
    {
        public static List<MockEntity> CreatedEntities { get; set; }
        public static List<MockEntity> ModifiedOriginalEntities { get; set; }
        public static List<MockEntity> ModifiedNewEntities { get; set; }
        public static List<MockEntity> RemovedEntities { get; set; }

        public new static void Reset()
        {
            CreatedEntities = new List<MockEntity>();
            ModifiedNewEntities = new List<MockEntity>();
            ModifiedOriginalEntities = new List<MockEntity>();
            RemovedEntities = new List<MockEntity>();
            Listener<MockEntity>.Reset();
        }

        public override async Task OnCreatedAsync(MockEntity entity, IContextInfo context)
        {
            await base.OnCreatedAsync(entity, context);
            CreatedEntities.Add(entity);
        }

        public override async Task OnModifiedAsync(MockEntity originalEntity, MockEntity modifiedEntity, IContextInfo context)
        {
            await base.OnModifiedAsync(originalEntity, modifiedEntity, context);
            ModifiedOriginalEntities.Add(originalEntity);
            ModifiedNewEntities.Add(modifiedEntity);
        }

        public override async Task OnRemovedAsync(MockEntity entity, IContextInfo context)
        {
            await base.OnRemovedAsync(entity, context);
            RemovedEntities.Add(entity);
        }
    }
}