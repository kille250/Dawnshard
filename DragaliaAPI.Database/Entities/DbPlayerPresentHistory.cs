﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DragaliaAPI.Shared.Definitions.Enums;
using DragaliaAPI.Shared.Features.Presents;
using Microsoft.EntityFrameworkCore;

namespace DragaliaAPI.Database.Entities;

[Table("PlayerPresentHistory")]
[Index(nameof(DeviceAccountId))]
public class DbPlayerPresentHistory : IDbHasAccountId
{
    /// <inheritdoc />
    public virtual DbPlayer? Owner { get; set; }

    /// <inheritdoc />
    [ForeignKey(nameof(Owner))]
    public required string DeviceAccountId { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }

    [Column("EntityType")]
    [TypeConverter(typeof(EnumConverter))]
    public EntityTypes EntityType { get; set; }

    [Column("EntityId")]
    public int EntityId { get; set; }

    [Column("EntityQuantity")]
    public int EntityQuantity { get; set; }

    [Column("EntityLevel")]
    public int EntityLevel { get; set; }

    [Column("EntityLimitBreakCount")]
    public int EntityLimitBreakCount { get; set; }

    [Column("EntityStatusPlusCount")]
    public int EntityStatusPlusCount { get; set; }

    [Column("MessageId")]
    public PresentMessage MessageId { get; set; }

    [Column("MessageParamValue1")]
    public int MessageParamValue1 { get; set; }

    [Column("MessageParamValue2")]
    public int MessageParamValue2 { get; set; }

    [Column("MessageParamValue3")]
    public int MessageParamValue3 { get; set; }

    [Column("MessageParamValue4")]
    public int MessageParamValue4 { get; set; }

    [Column("CreateTime")]
    [TypeConverter(typeof(DateTimeOffsetConverter))]
    public DateTimeOffset CreateTime { get; set; } = DateTimeOffset.UtcNow;
}
