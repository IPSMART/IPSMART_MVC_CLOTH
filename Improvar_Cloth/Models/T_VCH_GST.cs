namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_VCH_GST")]
    public partial class T_VCH_GST
    {
        public int? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(30)]
        public string AUTONO { get; set; }

        [StringLength(5)]
        public string DOCCD { get; set; }

        [StringLength(6)]
        public string DOCNO { get; set; }

        public DateTime? DOCDT { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DSLNO { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SLNO { get; set; }

        [StringLength(8)]
        public string PCODE { get; set; }

        [StringLength(20)]
        public string BLNO { get; set; }

        public DateTime? BLDT { get; set; }

        [StringLength(10)]
        public string HSNCODE { get; set; }

        [StringLength(50)]
        public string ITNM { get; set; }

        public double? AMT { get; set; }

        public double? CGSTPER { get; set; }

        public double? SGSTPER { get; set; }

        public double? IGSTPER { get; set; }

        public double? CGSTAMT { get; set; }

        public double? SGSTAMT { get; set; }

        public double? IGSTAMT { get; set; }

        public double? CESSPER { get; set; }

        public double? CESSAMT { get; set; }

        [StringLength(1)]
        public string DRCR { get; set; }

        public double? QNTY { get; set; }

        [StringLength(3)]
        public string UOM { get; set; }

        [StringLength(30)]
        public string AGSTDOCNO { get; set; }

        public DateTime? AGSTDOCDT { get; set; }

        [StringLength(1)]
        public string SALPUR { get; set; }

        [StringLength(2)]
        public string INVTYPECD { get; set; }

        [StringLength(2)]
        public string DNCNCD { get; set; }

        [StringLength(2)]
        public string EXPCD { get; set; }

        [StringLength(50)]
        public string GSTSLNM { get; set; }

        [StringLength(15)]
        public string GSTNO { get; set; }

        [StringLength(2)]
        public string POS { get; set; }

        [StringLength(16)]
        public string SHIPDOCNO { get; set; }

        public DateTime? SHIPDOCDT { get; set; }

        [StringLength(6)]
        public string PORTCD { get; set; }

        public double? OTHRAMT { get; set; }

        public double? ROAMT { get; set; }

        public double? BLAMT { get; set; }

        [StringLength(2)]
        public string DNCNSALPUR { get; set; }

        [StringLength(8)]
        public string CONSLCD { get; set; }

        public double? APPLTAXRATE { get; set; }

        [StringLength(1)]
        public string EXEMPTEDTYPE { get; set; }

        [StringLength(1)]
        public string GSTREGNTYPE { get; set; }

        [StringLength(1)]
        public string GOOD_SERV { get; set; }

        [StringLength(8)]
        public string EXPGLCD { get; set; }

        [StringLength(1)]
        public string INPTCLAIM { get; set; }
    }
}
