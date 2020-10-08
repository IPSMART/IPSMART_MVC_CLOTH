namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_BARCODEPRINTFORMAT")]
    public partial class M_BARCODEPRINTFORMAT
    {
        public short? EMD_NO { get; set; }

        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        public byte? SLNO { get; set; }

        [StringLength(100)]
        public string FMTTEXT { get; set; }

        [StringLength(1)]
        public string ALIGN { get; set; }

        public byte? FONTSZ { get; set; }

        [StringLength(1)]
        public string MARGINE { get; set; }
        
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long M_AUTONO { get; set; }
    }
}
