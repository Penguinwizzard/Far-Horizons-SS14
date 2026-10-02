using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.VariationPass;
using Content.Server.Storage.EntitySystems;
using Content.Server._FarHorizons.GameTicking.Rules.VariationPass.Components;
using Content.Shared.EntityTable;
using Content.Shared.Storage.Components;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._FarHorizons.GameTicking.Rules.VariationPass;

/// <summary>
/// Handles putting things in lockers around the station, intended for creatures.
/// </summary>
public sealed partial class MaintenanceMonstersVariationPassSystem : VariationPassSystem<MaintenanceMonstersVariationPassComponent>
{
    [Dependency] private EntityStorageSystem _entityStorage = default!;
    [Dependency] private EntityTableSystem _entityTable = default!;
    [Dependency] private IRobustRandom _random = default!;

    protected override void ApplyVariation(Entity<MaintenanceMonstersVariationPassComponent> ent, ref StationVariationPassEvent args)
    {
        var query = AllEntityQuery<MonsterSpawnCapableComponent, EntityStorageComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var storage, out var transform))
        {
            // Ignore if not part of the station
            if (!IsMemberOfStation((uid, transform), ref args))
                continue;

            // If we don't hit the random chance for this one, skip
            if (!_random.Prob(ent.Comp.PerLockerProbability))
                continue;

            var protos = _entityTable.GetSpawns(ent.Comp.SpawnTable);

            foreach (var proto in protos)
            {
                var spawn = Spawn(proto, MapCoordinates.Nullspace);
                if (!_entityStorage.Insert(spawn, uid, storage))
                {
                    Del(spawn);
                    // We failed to put one in the storage, so we're likely to fail at putting the rest in; go to the next storage.
                    break;
                }
            }
        }
    }
}
