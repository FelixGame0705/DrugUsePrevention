using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BussinessObjects
{
    public class NewsTag
    {
        [Key, Column(Order = 0)]
        public int NewsArticleID { get; set; }

        [Key, Column(Order = 1)]
        public int TagID { get; set; }

        [ForeignKey("NewsArticleID")]
        public virtual NewsArticle NewsArticle { get; set; }

        [ForeignKey("TagID")]
        public virtual Tag Tag { get; set; }
    }
}
