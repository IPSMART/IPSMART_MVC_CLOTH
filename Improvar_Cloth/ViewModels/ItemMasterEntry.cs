﻿using Improvar.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;

namespace Improvar.ViewModels
{
    public class ItemMasterEntry : Permission
    {
        //public string MSG { get; set; }
        public M_SITEM M_SITEM { get; set; }
        public M_GROUP M_GROUP { get; set; }
        public M_BRAND M_BRAND { get; set; }
        public M_COLOR M_COLOR { get; set; }
        public M_COLLECTION M_COLLECTION { get; set; }
        public M_SUBBRAND M_SUBBRAND { get; set; }
        public M_UOM M_UOM { get; set; }
        //public List<IndexKey1> IndexKey1 { get; set; }
        //public List<IndexKey12> IndexKey12 { get; set; }
        //public List<IndexKey13> IndexKey13 { get; set; }
        //public List<IndexKey14> IndexKey14 { get; set; }
        //public List<IndexKey15> IndexKey15 { get; set; }
        //public List<IndexKey01> IndexKey01 { get; set; }
        public List<Gender> Gender { get; set; }
        public List<ProductType> ProductType { get; set; }
        public List<MSITEMCOLOR> MSITEMCOLOR { get; set; }
        public List<MSITEMPARTS> MSITEMPARTS { get; set; }
        public List<MSITEMBARCODE> MSITEMBARCODE { get; set; }
        public M_SITEM_SIZE M_SITEM_SIZE { get; set; }
        public List<MSITEMSIZE> MSITEMSIZE { get; set; }
        //public List<MSITEMBOX> MSITEMBOX { get; set; }
        public List<MSITEMMEASURE> MSITEMMEASURE { get; set; }
        public List<DOCTYPE> Document { get; set; }
        public M_CNTRL_HDR M_CNTRL_HDR { get; set; }
        public M_CNTRL_HDR_DOC M_CNTRL_HDR_DOC { get; set; }
        public List<DocumentType> DocumentType { get; set; }
        public List<Database_Combo1> Database_Combo1 { get; set; }
        public List<Database_Combo2> Database_Combo2 { get; set; }
        public bool Checked { get; set; }
        public string FABITNM { get; set; }
        public string FABUOMNM { get; set; }        
        public string PRICES_EFFDT { get; set; }
        public string FABSTYLENO { get; set; }
        public DataTable DTPRICES { get; set; }
        public string SEARCH_ITCD { get; set; }

    }
}