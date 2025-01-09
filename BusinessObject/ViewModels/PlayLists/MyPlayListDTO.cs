using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.ViewModels.PlayLists
{
    public class MyPlayListDTO
    {
        public Guid PlayListId { get; set; }
        public string? PlayListName { get; set; }
        public List<MyPlayListItemDTO> PlayListItems { get; set; } = new();
    }

    public class MyPlayListItemDTO
    {
        public Guid PlaylistItemId { get; set; }
        public Guid RecapId { get; set; }
        public int OrderPlayList { get; set; }
        public string? RecapName { get; set; }
        public bool? isPremium { get; set; }
        public int? LikesCount { get; set; }
        public int? ViewsCount { get; set; }
        public string? ContributorName { get; set; }
        public string? ContributorImage { get; set; }
        public string? BookName { get; set; }
        public string? BookImage {  get; set; }
        public List<AuthorPlayListDTO> Authors { get; set; } = new();
    }
    public class AuthorPlayListDTO
    {
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorImage { get; set; }
        public string AuthorDescription { get; set; }
    }

}
