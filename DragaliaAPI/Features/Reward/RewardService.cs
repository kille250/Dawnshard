﻿using System.Diagnostics;
using DragaliaAPI.Database.Entities;
using DragaliaAPI.Database.Repositories;
using DragaliaAPI.Features.Dmode;
using DragaliaAPI.Features.Event;
using DragaliaAPI.Features.Fort;
using DragaliaAPI.Features.Item;
using DragaliaAPI.Features.Player;
using DragaliaAPI.Features.Reward.Handlers;
using DragaliaAPI.Features.Talisman;
using DragaliaAPI.Features.Tickets;
using DragaliaAPI.Models.Generated;
using DragaliaAPI.Shared.Definitions.Enums;
using Microsoft.EntityFrameworkCore;

namespace DragaliaAPI.Features.Reward;

public class RewardService(
    ILogger<RewardService> logger,
    IUnitRepository unitRepository,
    IEnumerable<IRewardHandler> rewardHandlers
) : IRewardService
{
    private readonly List<Entity> discardedEntities = new();
    private readonly List<Entity> presentEntites = new();
    private readonly List<Entity> presentLimitEntities = new();
    private readonly List<Entity> newEntities = new();
    private readonly List<ConvertedEntity> convertedEntities = new();

    public async Task<RewardGrantResult> GrantReward(Entity entity)
    {
        if (entity.Quantity <= 0)
        {
            // NOTE: Should this be an invalid case?
            return RewardGrantResult.Added;
        }

        logger.LogTrace("Granting reward {@rewardEntity}", entity);

        RewardGrantResult result = await GrantRewardInternal(entity);

        return result;
    }

    public async Task GrantRewards(IEnumerable<Entity> entities)
    {
        entities = entities.ToList();
        logger.LogTrace("Granting rewards: {@rewards}", entities);

        foreach (Entity entity in entities)
        {
            RewardGrantResult result = await GrantRewardInternal(entity);
        }
    }

    private async Task<RewardGrantResult> GrantRewardInternal(Entity entity)
    {
        IRewardHandler? handler = rewardHandlers.SingleOrDefault(
            x => x.SupportedTypes.Contains(entity.Type)
        );

        if (handler is null)
        {
            logger.LogError("Failed to find reward handler for entity {@entity}", entity);
            throw new InvalidOperationException("Failed to grant reward");
        }

        GrantReturn grantReturn = await handler.Grant(entity);

        switch (grantReturn.Result)
        {
            case RewardGrantResult.Added:
                this.newEntities.Add(entity);
                break;
            case RewardGrantResult.Converted:
                ArgumentNullException.ThrowIfNull(grantReturn.ConvertedEntity);
                this.convertedEntities.Add(
                    new ConvertedEntity(entity, grantReturn.ConvertedEntity)
                );
                break;
            case RewardGrantResult.Discarded:
                this.discardedEntities.Add(entity);
                break;
            case RewardGrantResult.GiftBoxDiscarded:
                this.presentLimitEntities.Add(entity);
                break;
            case RewardGrantResult.GiftBox:
                this.presentEntites.Add(entity);
                break;
            case RewardGrantResult.Limit:
                break;
            case RewardGrantResult.FailError:
                logger.LogError("Granting of entity {@entity} failed.", entity);
                throw new InvalidOperationException("Failed to grant reward");
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    string.Empty,
                    "RewardGrantResult out of range"
                );
        }

        return grantReturn.Result;
    }

    public async Task<(RewardGrantResult Result, DbTalisman? Talisman)> GrantTalisman(
        Talismans id,
        int abilityId1,
        int abilityId2,
        int abilityId3,
        int hp,
        int atk
    )
    {
        // int currentCount = await unitRepository.Talismans.CountAsync();

        if (
            false /*TODO: currentCount >= TalismanService.TalismanMaxCount once we get presents working with it*/
        )
        {
            Entity coinReward = new(EntityTypes.Rupies, 0, TalismanService.TalismanCoinReward);
            await GrantReward(coinReward);

            convertedEntities.Add(
                new ConvertedEntity(new Entity(EntityTypes.Talisman, (int)id), coinReward)
            );

            return (RewardGrantResult.Converted, null);
        }

        DbTalisman talisman = unitRepository.AddTalisman(
            id,
            abilityId1,
            abilityId2,
            abilityId3,
            hp,
            atk
        );

        return (RewardGrantResult.Added, talisman);
    }

    public EntityResult GetEntityResult()
    {
        return new()
        {
            new_get_entity_list = newEntities.Select(x => x.ToDuplicateEntityList()),
            converted_entity_list = convertedEntities.Select(x => x.ToConvertedEntityList()),
            over_discard_entity_list = discardedEntities.Select(
                x => x.ToBuildEventRewardEntityList()
            ),
            over_present_entity_list = this.presentEntites.Select(
                x => x.ToBuildEventRewardEntityList()
            ),
            over_present_limit_entity_list = this.presentLimitEntities.Select(
                x => x.ToBuildEventRewardEntityList()
            ),
        };
    }
}
