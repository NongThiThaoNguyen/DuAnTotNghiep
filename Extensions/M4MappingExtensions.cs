using DuAnTotNghiep.DTOs.Objective;
using DuAnTotNghiep.DTOs.Topic;
using DuAnTotNghiep.Models;
using DuAnTotNghiep.ViewModels.Admin.Levels;
using DuAnTotNghiep.ViewModels.Admin.Objectives;
using DuAnTotNghiep.ViewModels.Admin.Skills;
using System.Linq;

namespace DuAnTotNghiep.Extensions
{
    public static class M4MappingExtensions
    {
        // ------------------ TOPIC ------------------

        public static TopicTreeDto ToTreeDto(this LearningTopic topic)
        {
            return new TopicTreeDto
            {
                Id = topic.Id,
                Name = topic.Title,
                ParentTopicId = topic.ParentTopicId,
                OrderIndex = topic.OrderIndex,
                // Children will be mapped recursively in Service
            };
        }

        public static TopicOptionDto ToOptionDto(this LearningTopic topic)
        {
            return new TopicOptionDto
            {
                Id = topic.Id,
                Name = topic.Title
            };
        }

        public static TopicDetailDto ToDetailDto(this LearningTopic topic)
        {
            return new TopicDetailDto
            {
                Id = topic.Id,
                Name = topic.Title,
                Description = topic.Description,
                SkillName = topic.Skill?.SkillName ?? string.Empty,
                LevelName = topic.Level?.Name ?? string.Empty,
                ParentTopicName = topic.ParentTopic?.Title,
                ObjectiveCount = topic.LearningObjectives?.Count ?? 0,
                IsActive = topic.Status == "Active"
            };
        }

        // ------------------ OBJECTIVE ------------------

        public static ObjectiveDetailDto ToDetailDto(this LearningObjective objective)
        {
            return new ObjectiveDetailDto
            {
                Id = objective.Id,
                TopicId = objective.TopicId,
                CognitiveLevel = objective.CognitiveLevel,
                ObjectiveText = objective.ObjectiveText,
                TopicName = objective.Topic?.Title ?? string.Empty,
                OrderIndex = objective.OrderIndex
            };
        }

        public static ObjectiveCreateViewModel ToCreateViewModel(this LearningObjective objective)
        {
            return new ObjectiveCreateViewModel
            {
                TopicId = objective.TopicId,
                Code = objective.CognitiveLevel,
                Title = objective.ObjectiveText,
                OrderIndex = objective.OrderIndex,
                IsActive = true
            };
        }

        public static ObjectiveEditViewModel ToEditViewModel(this LearningObjective objective)
        {
            return new ObjectiveEditViewModel
            {
                Id = objective.Id,
                TopicId = objective.TopicId,
                Code = objective.CognitiveLevel,
                Title = objective.ObjectiveText,
                OrderIndex = objective.OrderIndex,
                IsActive = true
            };
        }

        // ------------------ SKILL ------------------

        public static SkillCreateViewModel ToCreateViewModel(this EnglishSkill skill)
        {
            return new SkillCreateViewModel
            {
                Code = skill.SkillCode,
                Name = skill.SkillName,
                Description = skill.Description,
                IsActive = skill.IsActive
            };
        }

        public static SkillEditViewModel ToEditViewModel(this EnglishSkill skill)
        {
            return new SkillEditViewModel
            {
                Id = skill.Id,
                Code = skill.SkillCode,
                Name = skill.SkillName,
                Description = skill.Description,
                IsActive = skill.IsActive
            };
        }

        // ------------------ LEVEL ------------------

        public static LevelCreateViewModel ToCreateViewModel(this EnglishProficiencyLevel level)
        {
            return new LevelCreateViewModel
            {
                Code = level.Code,
                Name = level.Name,
                Description = level.Description,
                OrderIndex = level.OrderIndex,
                IsActive = level.IsActive
            };
        }

        public static LevelEditViewModel ToEditViewModel(this EnglishProficiencyLevel level)
        {
            return new LevelEditViewModel
            {
                Id = level.Id,
                Code = level.Code,
                Name = level.Name,
                Description = level.Description,
                OrderIndex = level.OrderIndex,
                IsActive = level.IsActive
            };
        }
    }
}
