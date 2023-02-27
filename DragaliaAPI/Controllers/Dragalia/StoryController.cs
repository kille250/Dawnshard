﻿using DragaliaAPI.Database.Repositories;
using DragaliaAPI.Models;
using DragaliaAPI.Models.Generated;
using DragaliaAPI.Services;
using DragaliaAPI.Shared.Definitions.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DragaliaAPI.Controllers.Dragalia;

[Route("story")]
public class StoryController : DragaliaControllerBase
{
    private readonly IStoryService storyService;
    private readonly IUpdateDataService updateDataService;
    private readonly ILogger<StoryController> logger;

    public StoryController(
        IStoryService storyService,
        IUpdateDataService updateDataService,
        ILogger<StoryController> logger
    )
    {
        this.storyService = storyService;
        this.updateDataService = updateDataService;
        this.logger = logger;
    }

    [HttpPost("read")]
    public async Task<DragaliaResult> Read(StoryReadRequest request)
    {
        if (!await this.storyService.CheckUnitStoryEligibility(request.unit_story_id))
        {
            this.logger.LogWarning("User did not have access to story {id}", request.unit_story_id);
            return this.Code(ResultCode.StoryNotReadThePrevious);
        }

        IEnumerable<AtgenBuildEventRewardEntityList> rewards =
            await this.storyService.ReadUnitStory(request.unit_story_id);

        UpdateDataList updateDataList = await this.updateDataService.SaveChangesAsync();

        return this.Ok(
            new StoryReadData()
            {
                unit_story_reward_list = rewards,
                update_data_list = updateDataList,
            }
        );
    }
}
