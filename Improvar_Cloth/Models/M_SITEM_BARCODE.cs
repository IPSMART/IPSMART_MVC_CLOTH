namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SITEM_BARCODE")]
    public partial class M_SITEM_BARCODE
    {
   
        public short? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [StringLength(10)]
        public string ITCD { get; set; }

        [Key]
        [StringLength(25)]
        public string BARNO { get; set; }

        [StringLength(4)]
        public string SIZECD { get; set; }

        [StringLength(4)]
        public string COLRCD { get; set; }

         public double? STDRT { get; set; }

        [StringLength(1)]
        public string INACTIVE_TAG { get; set; }
    }
}
