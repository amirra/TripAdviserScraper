//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TripAdviserScraper.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class LocationComment
    {
        public long ID { get; set; }
        public string Title_Original { get; set; }
        public string Description_Original { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Nullable<long> LocationRef { get; set; }
    
        public virtual Location Location { get; set; }
    }
}
