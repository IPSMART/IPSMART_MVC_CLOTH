﻿using Improvar.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Improvar.ViewModels
{
    public class RepMiscQryUpdt :Permission
    {
        public List<DropDown_list1> DropDown_list1 { get; set; }
        public string BALEYR2 { get; set; }
        public string BALEYR1 { get; set; }
        public string BALENO2 { get; set; }
        public string BALENO1 { get; set; }
        public string TEXTBOX1 { get; set; }
        public string LRNO1 { get; set; }
        public string LRNO2 { get; set; }
        public string LRDT1 { get; set; }
        public string LRDT2 { get; set; }
        public string PBLNO2 { get; set; }
        public string BLAUTONO1 { get; set; }
        public string BLAUTONO2 { get; set; }
        public string OLDSTYLENO { get; set; }
        public string OLDPAGENO { get; set; }
        public string NEWPAGENO { get; set; }
        public string NEWSTYLENO { get; set; }
        public string NEWPAGESLNO { get; set; }
        public string OLDPAGESLNO { get; set; }
        public string BLSLNO1 { get; set; }
        public string BLSLNO2 { get; set; }
        public string ITCD1 { get; set; }
        public string GOCD1 { get; set; }
        public string ITCD2 { get; set; }
        public string GOCD2 { get; set; }
        public string M_BARCODE { get; set; }
        public string MTRLJOBCD { get; set; }
        public string PARTCD { get; set; }
        public string DOCDT { get; set; }
        public string TAXGRPCD { get; set; }
        public string PRCCD { get; set; }
        public string ALLMTRLJOBCD { get; set; }
        

    }
}