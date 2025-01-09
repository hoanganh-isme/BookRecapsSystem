﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Models
{
    public class Recap : BaseEntity
    {
        public string? Name { get; set; }
        public bool isPublished {  get; set; }
        public bool isPremium {  get; set; }
        public int? LikesCount { get; set; }
        public int? ViewsCount {  get; set; }
        public Guid UserId { get; set; }
        public User Contributor { get; set; }
        public Guid BookId { get; set; }
        public Book Book { get; set; }
        public Guid? CurrentVersionId { get; set; }
        public RecapVersion CurrentVersion { get; set; }
        public ICollection<RecapVersion> RecapVersions { get; set; }
        public ICollection<PlayListItem> PlayListItems { get; set; }
        public ICollection<ViewTracking> ViewTrackings { get; set; }
        public ICollection<SupportTicket> SupportTicketItems { get; set; }
        public ICollection<RecapEarning> RecapEarnings { get; set; }
        public ICollection<Like> Likes { get; set; }

    }
}