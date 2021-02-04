﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Globalization;
using Oracle.ManagedDataAccess.Client;
//using NDbfReader;
namespace Improvar.Controllers
{
    public class T_DataUploadController : Controller
    {
        string CS = null; string sql = ""; string dbsql = ""; string[] dbsql1;
        string dberrmsg = "";
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        Salesfunc Salesfunc = new Salesfunc();
        private ImprovarDB DB, DBF;
        public T_DataUploadController()
        {
            DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
        }
        // GET: T_DataUpload
        public ActionResult T_DataUpload()
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                ViewBag.formname = "Data Upload";
                DataUploadVM VE = new DataUploadVM();
                return View(VE);
            }
        }
        [HttpPost]
        public ActionResult T_DataUpload(DataUploadVM VE, FormCollection FC, String Command)
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Request.Files.Count == 0) return Content("No File Selected");
            VE = ReadRaymondPurchaseDBF(VE);
            return View(VE);
        }
        public DataUploadVM ReadRaymondPurchaseDBF(DataUploadVM VE)
        {
            List<DUpGrid> DUGridlist = new List<DUpGrid>();
            try
            {
                string Path = "C:\\IPSMART\\Temp";
                if (!System.IO.Directory.Exists(Path)) { System.IO.Directory.CreateDirectory(Path); }
                Path = "C:\\IPSMART\\Temp\\Raymond.dbf";
                if (System.IO.File.Exists(Path)) { System.IO.File.Delete(Path); }
                GC.Collect();
                Request.Files["FileUpload"].SaveAs(Path);

                System.Data.Odbc.OdbcConnection obdcconn = new System.Data.Odbc.OdbcConnection();
                obdcconn.ConnectionString = "Driver={Microsoft dBase Driver (*.dbf)};SourceType=DBF;SourceDB=" + Path + ";Exclusive=No; NULL=NO;DELETED=NO;BACKGROUNDFETCH=NO;";
                obdcconn.Open();
                System.Data.Odbc.OdbcCommand oCmd = obdcconn.CreateCommand();
                oCmd.CommandText = "SELECT * FROM " + Path;
                DataTable dbfdt = new DataTable();
                dbfdt.Load(oCmd.ExecuteReader());
                obdcconn.Close();
                if (System.IO.File.Exists(Path)) { System.IO.File.Delete(Path); }

                TransactionSaleEntry TMPVE = new TransactionSaleEntry();
                T_SALEController TSCntlr = new T_SALEController();
                T_TXN TTXN = new T_TXN();
                T_TXNTRANS TXNTRANS = new T_TXNTRANS();
                T_TXNOTH TTXNOTH = new T_TXNOTH();
                TMPVE.DefaultAction = "A";
                TMPVE.MENU_PARA = "PB";
                var outerDT = dbfdt.AsEnumerable()
               .GroupBy(g => new { CUSTOMERNO = g["CUSTOMERNO"], INV_NO = g["INV_NO"], INVDATE = g["INVDATE"], LR_NO = g["LR_NO"], LR_DATE = g["LR_DATE"], CARR_NO = g["CARR_NO"], CARR_NAME = g["CARR_NAME"] })
               .Select(g =>
               {
                   var row = dbfdt.NewRow();
                   row["CUSTOMERNO"] = g.Key.CUSTOMERNO;
                   row["INV_NO"] = g.Key.INV_NO;
                   row["INVDATE"] = g.Key.INVDATE;
                   row["LR_NO"] = g.Key.LR_NO;
                   row["LR_DATE"] = g.Key.LR_DATE;
                   row["CARR_NO"] = g.Key.CARR_NO;
                   row["CARR_NAME"] = g.Key.CARR_NAME;
                   row["FREIGHT"] = g.Sum(r => r.Field<double>("FREIGHT"));
                   row["INSURANCE"] = g.Sum(r => r.Field<double>("INSURANCE"));
                   row["INV_VALUE"] = g.Sum(r => r.Field<double>("INV_VALUE"));
                   row["NET_AMT"] = g.Sum(r => r.Field<double>("NET_AMT"));
                   row["TAX_AMT"] = g.Sum(r => r.Field<double>("TAX_AMT"));
                   row["INTEGR_TAX"] = g.Average(r => r.Field<double>("INTEGR_TAX"));
                   row["INTEGR_AMT"] = g.Sum(r => r.Field<double>("INTEGR_AMT"));
                   row["CENT_TAX"] = g.Average(r => r.Field<double>("CENT_TAX"));
                   row["CENT_AMT"] = g.Sum(r => r.Field<double>("CENT_AMT"));
                   row["STATE_TAX"] = g.Average(r => r.Field<double>("STATE_TAX"));
                   row["STATE_AMT"] = g.Sum(r => r.Field<double>("STATE_AMT"));
                   return row;
               }).CopyToDataTable();

                TTXN.EMD_NO = 0;
                TTXN.DOCCD = DB.M_DOCTYPE.Where(d => d.DOCTYPE == "SPBL").FirstOrDefault()?.DOCCD;
                TTXN.CLCD = CommVar.ClientCode(UNQSNO);
                short slno = 0;
                foreach (DataRow oudr in outerDT.Rows)
                {
                    List<TBATCHDTL> TBATCHDTLlist = new List<Models.TBATCHDTL>();
                    List<TTXNDTL> TTXNDTLlist = new List<Models.TTXNDTL>();
                    List<TTXNAMT> TTXNAMTlist = new List<Models.TTXNAMT>();
                    DUpGrid dupgrid = new DUpGrid();
                    TTXN.GOCD = "TR";
                    TTXN.DOCTAG = "PB";
                    TTXN.PREFNO = oudr["INV_NO"].ToString();
                    TTXN.TCSPER = 0.075;
                    dupgrid.BLNO = TTXN.PREFNO;
                    string Ddate = DateTime.ParseExact(oudr["INVDATE"].retDateStr(), "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
                    TTXN.DOCDT = Convert.ToDateTime(Ddate);
                    dupgrid.BLDT = Ddate;
                    TTXN.PREFDT = TTXN.DOCDT;
                    dupgrid.BLNO = TTXN.PREFNO;
                    string CUSTOMERNO = oudr["CUSTOMERNO"].ToString();
                    TTXN.SLCD = getSLCD(CUSTOMERNO); dupgrid.CUSTOMERNO = CUSTOMERNO;
                    if (TTXN.SLCD == "")
                    {
                        dupgrid.MESSAGE = "Please add Customer No:(" + CUSTOMERNO + ") in the SAPCODE from [Tax code link up With Party].";
                        DUGridlist.Add(dupgrid);
                        break;
                    }
                    double igstper = oudr["INTEGR_TAX"].retDbl();
                    double cgstper = oudr["CENT_AMT"].retDbl();
                    double gstper = igstper == 0 ? (cgstper * 2) : igstper;
                    double blINV_VALUE = oudr["INV_VALUE"].retDbl();
                    double bltaxable = oudr["TAX_AMT"].retDbl();
                    double calculatedTax = Math.Round(((bltaxable * gstper) / 100), 2);
                    double calcultednet = (bltaxable + calculatedTax);//.toRound(2);
                    var roffamt = (blINV_VALUE - calcultednet).toRound(2);
                    double blTAX_AMT = oudr["TAX_AMT"].retDbl();
                    double tcsamt = (blINV_VALUE * TTXN.TCSPER.retDbl() / 100).toRound(2);
                    TTXN.BLAMT = blINV_VALUE + tcsamt;
                    TTXN.TDSCODE = "X";
                    TTXN.ROYN = "Y";
                    TTXN.TCSON = calcultednet;
                    TTXN.TCSAMT = tcsamt; dupgrid.TCSAMT = tcsamt.ToString();
                    sql = "";
                    sql = "select a.autono,b.docno,a.SLCD,a.blamt,a.tcsamt  from  " + CommVar.CurSchema(UNQSNO) + ".t_txn a, " + CommVar.CurSchema(UNQSNO) + ".t_cntrl_hdr b ";
                    sql += " where   a.autono=b.autono and a.PREFNO='" + TTXN.PREFNO + "' and a.slcd='" + TTXN.SLCD + "' ";
                    var dt = masterHelp.SQLquery(sql);
                    if (dt.Rows.Count > 0)
                    {
                        dupgrid.MESSAGE = "Allready Added " + dt.Rows[0]["docno"].ToString();
                        dupgrid.BLNO = dt.Rows[0]["docno"].ToString();
                        dupgrid.TCSAMT = dt.Rows[0]["tcsamt"].ToString();
                        dupgrid.BLAMT = dt.Rows[0]["blamt"].ToString();
                        DUGridlist.Add(dupgrid);
                        continue;
                    }

                    //-------------------------Transport--------------------------//

                    if (oudr["CARR_NO"].ToString() != "")
                    {
                        TXNTRANS.TRANSLCD = getSLCD(oudr["CARR_NO"].ToString());
                        if (TXNTRANS.TRANSLCD == "")
                        {
                            dupgrid.MESSAGE = "Please add  CARR_NO:(" + oudr["CARR_NO"].ToString() + ")/ Transporter,CARR_NAME:(" + oudr["CARR_NAME"].ToString() + ") in the SAPCODE from [Tax code link up With Party].";
                            DUGridlist.Add(dupgrid); break;
                        }
                    }

                    string LR_DATE = DateTime.ParseExact(oudr["LR_DATE"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
                    TXNTRANS.LRNO = oudr["LR_NO"].ToString();
                    TXNTRANS.LRDT = Convert.ToDateTime(LR_DATE);
                    //----------------------------------------------------------//
                    string PURGLCD = "";

                    DataTable innerDt = dbfdt.Select("INV_NO='" + TTXN.PREFNO + "'").CopyToDataTable();
                    double txable = 0, gstamt = 0;
                    foreach (DataRow inrdr in innerDt.Rows)
                    {
                        double amttabigstamt = 0; double amttabcgstamt = 0;
                        //Amount tab start
                        if (inrdr["FREIGHT"].retDbl() != 0)
                        {
                            if (igstper > 0)
                            {
                                amttabigstamt += (inrdr["FREIGHT"].retDbl() * igstper / 100).toRound(2);
                            }
                            else
                            {
                                amttabcgstamt += (inrdr["FREIGHT"].retDbl() * cgstper / 100).toRound(2);
                            }
                        }
                        if (inrdr["INSURANCE"].retDbl() != 0)
                        {
                            if (igstper > 0)
                            {
                                amttabigstamt += (inrdr["INSURANCE"].retDbl() * igstper / 100).toRound(2);
                            }
                            else
                            {
                                amttabcgstamt += (inrdr["INSURANCE"].retDbl() * cgstper / 100).toRound(2);
                            }
                        }
                        //detail tab start
                        TTXNDTL TTXNDTL = new TTXNDTL();
                        string style = inrdr["MAT_GRP"].ToString() + inrdr["MAT_GRP"].ToString().Split('-')[0];
                        string grpnm = inrdr["MAT_DESCRI"].ToString();
                        string HSNCODE = inrdr["HSN_CODE"].ToString();
                        ItemDet ItemDet = CreateItem(style, "MTR", grpnm, HSNCODE);
                        TTXNDTL.ITCD = ItemDet.ITCD; PURGLCD = ItemDet.PURGLCD;
                        TTXNDTL.SLNO = ++slno;
                        TTXNDTL.MTRLJOBCD = "FS";

                        TTXNDTL.STKDRCR = "D";
                        TTXNDTL.STKTYPE = "F";
                        TTXNDTL.HSNCODE = HSNCODE;
                        //TTXNDTL.ITREM = VE.TTXNDTL[i].ITREM;
                        //TTXNDTL.BATCHNO = inrdr["BATCH"].ToString();
                        TTXNDTL.BALENO = inrdr["BALENO"].ToString();
                        TTXNDTL.GOCD = "TR";
                        TTXNDTL.QNTY = inrdr["NET_QTY"].retDbl();
                        TTXNDTL.NOS = 1;
                        TTXNDTL.RATE = inrdr["RATE"].retDbl();
                        TTXNDTL.AMT = inrdr["GROSS_AMT"].retDbl();
                        TTXNDTL.FLAGMTR = inrdr["W_FLG_Q"].retDbl();
                        string grade = inrdr["GRADATION"].ToString();
                        string foc = inrdr["FOC"].ToString();
                        string pCSTYPE = PCSTYPE(grade, foc);
                        double W_FLG_Q = Math.Abs(inrdr["W_FLG_Q"].retDbl());
                        double R_FLG_Q = Math.Abs(inrdr["R_FLG_Q"].retDbl());
                        double discamt = Math.Abs(inrdr["QLTY_DISC"].retDbl());
                        double discamt1 = Math.Abs(inrdr["MKTG_DISC"].retDbl());
                        double Flagamt = (W_FLG_Q + R_FLG_Q) * TTXNDTL.RATE.retDbl();
                        TTXNDTL.TOTDISCAMT = Flagamt;
                        TTXNDTL.DISCTYPE = "F";
                        TTXNDTL.DISCRATE = discamt;
                        TTXNDTL.DISCAMT = discamt;
                        TTXNDTL.SCMDISCTYPE = "F";
                        TTXNDTL.SCMDISCRATE = discamt1;
                        TTXNDTL.SCMDISCAMT = discamt1;
                        TTXNDTL.GLCD = PURGLCD;
                        TTXNDTL.TXBLVAL = inrdr["NET_AMT"].retDbl(); txable += TTXNDTL.TXBLVAL.retDbl();

                        double NET_AMT = ((TTXNDTL.TXBLVAL * (100 + gstper)) / 100).retDbl();
                        TTXNDTL.IGSTPER = inrdr["INTEGR_TAX"].retDbl();
                        TTXNDTL.CGSTPER = inrdr["CENT_TAX"].retDbl();
                        TTXNDTL.SGSTPER = inrdr["STATE_TAX"].retDbl();
                        TTXNDTL.GSTPER = TTXNDTL.IGSTPER.retDbl() + TTXNDTL.CGSTPER.retDbl() + TTXNDTL.SGSTPER.retDbl();

                        TTXNDTL.IGSTAMT = inrdr["INTEGR_AMT"].retDbl() - amttabigstamt; gstamt += TTXNDTL.IGSTAMT.retDbl();
                        TTXNDTL.CGSTAMT = inrdr["CENT_AMT"].retDbl() - amttabcgstamt; gstamt += TTXNDTL.CGSTAMT.retDbl();
                        TTXNDTL.SGSTAMT = inrdr["STATE_AMT"].retDbl() - amttabcgstamt; gstamt += TTXNDTL.SGSTAMT.retDbl();
                        TTXNDTL.NETAMT = NET_AMT.toRound(2);
                        TTXNDTLlist.Add(TTXNDTL);


                        TBATCHDTL TBATCHDTL = new TBATCHDTL();
                        TBATCHDTL.TXNSLNO = TTXNDTL.SLNO;
                        TBATCHDTL.SLNO = TTXNDTL.SLNO;  //COUNTER.retShort();
                        //TBATCHDTL.GOCD = VE.T_TXN.GOCD;
                        //TBATCHDTL.BARNO = barno;
                        TBATCHDTL.ITCD = TTXNDTL.ITCD;
                        TBATCHDTL.MTRLJOBCD = TTXNDTL.MTRLJOBCD;
                        TBATCHDTL.PARTCD = TTXNDTL.PARTCD;
                        TBATCHDTL.HSNCODE = TTXNDTL.HSNCODE;
                        TBATCHDTL.STKDRCR = TTXNDTL.STKDRCR;
                        TBATCHDTL.NOS = TTXNDTL.NOS;
                        TBATCHDTL.QNTY = TTXNDTL.QNTY;
                        TBATCHDTL.BLQNTY = TTXNDTL.BLQNTY;
                        TBATCHDTL.FLAGMTR = TTXNDTL.FLAGMTR;
                        TBATCHDTL.ITREM = TTXNDTL.ITREM;
                        TBATCHDTL.RATE = TTXNDTL.RATE;
                        TBATCHDTL.DISCRATE = TTXNDTL.DISCRATE;
                        TBATCHDTL.DISCTYPE = TTXNDTL.DISCTYPE;
                        TBATCHDTL.SCMDISCRATE = TTXNDTL.SCMDISCRATE;
                        TBATCHDTL.SCMDISCTYPE = TTXNDTL.SCMDISCTYPE;
                        TBATCHDTL.TDDISCRATE = TTXNDTL.TDDISCRATE;
                        TBATCHDTL.TDDISCTYPE = TTXNDTL.TDDISCTYPE;
                        //TBATCHDTL.DIA = TTXNDTL.DIA;
                        //TBATCHDTL.CUTLENGTH = TTXNDTL.CUTLENGTH;
                        //TBATCHDTL.LOCABIN = TTXNDTL.LOCABIN;
                        //TBATCHDTL.SHADE = TTXNDTL.SHADE;
                        //TBATCHDTL.MILLNM = TTXNDTL.MILLNM;
                        TBATCHDTL.BATCHNO = inrdr["BATCH"].ToString();
                        TBATCHDTL.BALEYR = TTXNDTL.BALENO.retStr() == "" ? "" : TTXNDTL.BALEYR;
                        TBATCHDTL.BALENO = TTXNDTL.BALENO;
                        //if (VE.MENU_PARA == "SBPCK")
                        //{
                        //    TBATCHDTL.ORDAUTONO = TTXNDTL.ORDAUTONO;
                        //    TBATCHDTL.ORDSLNO = TTXNDTL.ORDSLNO;
                        //}
                        TBATCHDTL.LISTPRICE = TTXNDTL.LISTPRICE;
                        TBATCHDTL.LISTDISCPER = TTXNDTL.LISTDISCPER;
                        //TBATCHDTL.CUTLENGTH = TTXNDTL.CUTLENGTH;
                        TBATCHDTL.STKTYPE = TTXNDTL.STKTYPE;

                        //if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "OP") && VE.M_SYSCNFG.MNTNPCSTYPE == "Y")
                        //{
                        //    TBATCHDTL.PCSTYPE = TTXNDTL.PCSTYPE;
                        //}
                        TBATCHDTLlist.Add(TBATCHDTL);
                    }// inner loop of TTXNDTL
                     //Amount tab start
                    if (oudr["FREIGHT"].retDbl() != 0)
                    {
                        TTXNAMT TTXNAMT = new TTXNAMT();
                        TTXNAMT.SLNO = 1;
                        TTXNAMT.GLCD = PURGLCD;
                        TTXNAMT.AMTCD = "0001";
                        TTXNAMT.AMTDESC = "";
                        TTXNAMT.AMTRATE = oudr["FREIGHT"].retDbl();
                        TTXNAMT.HSNCODE = "";
                        TTXNAMT.AMT = TTXNAMT.AMTRATE; txable += TTXNAMT.AMT.retDbl();
                        if (igstper > 0)
                        {
                            TTXNAMT.IGSTPER = igstper;
                            TTXNAMT.IGSTAMT = (oudr["FREIGHT"].retDbl() * igstper / 100).toRound(2); gstamt += TTXNAMT.IGSTAMT.retDbl();
                        }
                        else
                        {
                            TTXNAMT.CGSTPER = cgstper;
                            TTXNAMT.CGSTAMT = (oudr["FREIGHT"].retDbl() * cgstper / 100).toRound(2); gstamt += TTXNAMT.CGSTAMT.retDbl();
                            TTXNAMT.SGSTPER = cgstper;
                            TTXNAMT.SGSTAMT = (oudr["FREIGHT"].retDbl() * cgstper / 100).toRound(2); gstamt += TTXNAMT.SGSTAMT.retDbl();
                        }
                        TTXNAMTlist.Add(TTXNAMT);
                    }
                    if (oudr["INSURANCE"].retDbl() != 0)
                    {
                        TTXNAMT TTXNAMT = new TTXNAMT();
                        TTXNAMT.SLNO = 2;
                        TTXNAMT.GLCD = PURGLCD;
                        TTXNAMT.AMTCD = "0002";
                        TTXNAMT.AMTDESC = "";
                        TTXNAMT.AMTRATE = oudr["INSURANCE"].retDbl();
                        TTXNAMT.HSNCODE = "";
                        TTXNAMT.AMT = TTXNAMT.AMTRATE; txable += TTXNAMT.AMT.retDbl();
                        if (igstper > 0)
                        {
                            TTXNAMT.IGSTAMT = (oudr["INSURANCE"].retDbl() * igstper / 100).toRound(2); gstamt += TTXNAMT.IGSTAMT.retDbl();
                        }
                        else
                        {
                            TTXNAMT.CGSTPER = cgstper;
                            TTXNAMT.CGSTAMT = (oudr["INSURANCE"].retDbl() * cgstper / 100).toRound(2); gstamt += TTXNAMT.CGSTAMT.retDbl();
                            TTXNAMT.SGSTPER = cgstper;
                            TTXNAMT.SGSTAMT = (oudr["INSURANCE"].retDbl() * cgstper / 100).toRound(2); gstamt += TTXNAMT.SGSTAMT.retDbl();
                        }
                        TTXNAMTlist.Add(TTXNAMT);
                    }
                    //           //Amount tab end
                    TTXN.ROAMT = (TTXN.BLAMT.retDbl() - (txable + gstamt + tcsamt)).toRound(2);

                    TMPVE.T_TXN = TTXN;
                    TMPVE.T_TXNTRANS = TXNTRANS;
                    TMPVE.T_TXNOTH = TTXNOTH;
                    TMPVE.TTXNDTL = TTXNDTLlist;
                    TMPVE.TBATCHDTL = TBATCHDTLlist;
                    TMPVE.TTXNAMT = TTXNAMTlist;
                    TMPVE.T_VCH_GST = new T_VCH_GST();
                    string tslCont = (string)TSCntlr.SAVE(TMPVE, "PosPurchase");
                    tslCont = tslCont.retStr().Split('~')[0];
                    if (tslCont.Length > 0 && tslCont.Substring(0, 1) == "1") dupgrid.MESSAGE = "Success " + tslCont.Substring(1);
                    else dupgrid.MESSAGE = tslCont;
                    DUGridlist.Add(dupgrid);
                }//outer


            }//try
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                VE.STATUS = ex.Message + ex.StackTrace;
            }
            VE.DUpGrid = DUGridlist;
            return VE;
        }

        public ItemDet CreateItem(string style, string UOM, string grpnm, string HSNCODE)
        {
            string DefaultAction = "A"; ItemDet ItemDet = new ItemDet();
            M_SITEM MSITEM = new M_SITEM(); M_GROUP MGROUP = new M_GROUP();
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            try
            {
                OraCon.Open();
                MSITEM.CLCD = CommVar.ClientCode(UNQSNO);
                var STYLEdt = (from g in DB.M_SITEM
                               join h in DB.M_GROUP on g.ITGRPCD equals h.ITGRPCD
                               where g.STYLENO == style
                               select new
                               {
                                   ITCD = g.ITCD,
                                   PURGLCD = h.PURGLCD,
                               }).FirstOrDefault();
                if (STYLEdt != null)
                {
                    ItemDet.ITCD = STYLEdt.ITCD;
                    ItemDet.PURGLCD = STYLEdt.PURGLCD;
                    return ItemDet;
                }
                MGROUP = CreateGroup(grpnm);
                MSITEM.EMD_NO = 0;
                MSITEM.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                string sql = "select max(itcd)itcd from " + CommVar.CurSchema(UNQSNO) + ".m_sitem where itcd like('" + MGROUP.ITGRPTYPE + MGROUP.GRPBARCODE + "%') ";
                var tbl = masterHelp.SQLquery(sql);
                if (tbl.Rows[0]["itcd"].ToString() == "")
                {
                    MSITEM.ITCD = MGROUP.ITGRPTYPE + MGROUP.GRPBARCODE + "00001";
                }
                else
                {
                    string s = tbl.Rows[0]["itcd"].ToString();
                    string digits = new string(s.Where(char.IsDigit).ToArray());
                    string letters = new string(s.Where(char.IsLetter).ToArray());
                    int number;
                    if (!int.TryParse(digits, out number))                   //int.Parse would do the job since only digits are selected
                    {
                        Console.WriteLine("Something weired happened");
                    }
                    MSITEM.ITCD = letters + (++number).ToString("D7");
                }
                MSITEM.ITGRPCD = MGROUP.ITGRPCD;
                MSITEM.ITNM = "";
                MSITEM.STYLENO = style.Trim();
                MSITEM.UOMCD = UOM;
                MSITEM.HSNCODE = HSNCODE;
                MSITEM.NEGSTOCK = "Y";
                var MPRODGRP = DB.M_PRODGRP.FirstOrDefault();
                MSITEM.PRODGRPCD = MPRODGRP?.PRODGRPCD;


                M_SITEM_BARCODE MSITEMBARCODE1 = new M_SITEM_BARCODE();
                MSITEMBARCODE1.EMD_NO = MSITEM.EMD_NO;
                MSITEMBARCODE1.CLCD = MSITEM.CLCD;
                MSITEMBARCODE1.ITCD = MSITEM.ITCD;
                MSITEMBARCODE1.BARNO = Salesfunc.GenerateBARNO(MSITEM.ITCD, "", "");

                DB.M_SITEM_BARCODE.Add(MSITEMBARCODE1);


                OracleCommand OraCmd = OraCon.CreateCommand();
                using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(false, "M_SITEM", MSITEM.M_AUTONO, DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                    dbsql = masterHelp.RetModeltoSql(MCH, "A", CommVar.CurSchema(UNQSNO));
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelp.RetModeltoSql(MSITEM, "A", CommVar.CurSchema(UNQSNO));
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelp.RetModeltoSql(MSITEMBARCODE1, "A", CommVar.CurSchema(UNQSNO));
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();


                    OraTrans.Commit();
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
            OraCon.Dispose();
            ItemDet.ITCD = MSITEM.ITCD;
            ItemDet.PURGLCD = MGROUP.PURGLCD;
            return ItemDet;
        }
        public M_GROUP CreateGroup(string grpnm)
        {
            M_GROUP MGROUP = new M_GROUP();
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            try
            {
                string DefaultAction = "A";
                var tMGROU = DB.M_GROUP.Where(m => m.ITGRPNM == grpnm).FirstOrDefault();
                if (tMGROU != null)
                {
                    return tMGROU;
                }
                MGROUP.CLCD = CommVar.ClientCode(UNQSNO);
                MGROUP.EMD_NO = 0;
                MGROUP.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());

                string txtst = grpnm.Substring(0, 1).Trim().ToUpper();
                MGROUP.ITGRPNM = grpnm.ToUpper();
                string sql = " select max(SUBSTR(ITGRPCD, 2)) ITGRPCD FROM " + CommVar.CurSchema(UNQSNO) + ".M_GROUP";
                string sql1 = " select max(GRPBARCODE) GRPBARCODE FROM " + CommVar.CurSchema(UNQSNO) + ".M_GROUP";
                var tbl = masterHelp.SQLquery(sql);
                if (tbl.Rows[0]["ITGRPCD"].ToString() != "")
                {
                    MGROUP.ITGRPCD = txtst + ((tbl.Rows[0]["ITGRPCD"]).retInt() + 1).ToString("D3");
                }
                else
                {
                    MGROUP.ITGRPCD = txtst + (10).ToString("D3");
                }
                var tb1l = masterHelp.SQLquery(sql1);
                if (tb1l.Rows[0]["GRPBARCODE"].ToString() != "")
                {
                    MGROUP.GRPBARCODE = ((tb1l.Rows[0]["GRPBARCODE"]).retInt() + 1).ToString("D2");
                }
                else
                {
                    MGROUP.GRPBARCODE = (10).ToString("D2");
                }
                MGROUP.SALGLCD = "10000001";
                MGROUP.PURGLCD = "25999991";
                MGROUP.ITGRPTYPE = "F";
                MGROUP.PRODGRPCD = "G001";
                MGROUP.BARGENTYPE = "C";
                OraCon.Open();
                OracleCommand OraCmd = OraCon.CreateCommand();
                using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(false, "M_GROUP", MGROUP.M_AUTONO, DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                    dbsql = masterHelp.RetModeltoSql(MCH, "A", CommVar.CurSchema(UNQSNO));
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelp.RetModeltoSql(MGROUP, "A", CommVar.CurSchema(UNQSNO));
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    OraTrans.Commit();
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
            OraCon.Dispose();
            return MGROUP;
        }
        public string PCSTYPE(string grade, string foc)
        {
            string pcstype = "";
            switch (grade)
            {
                case "G":
                    grade = "GOOD"; break;
                case "B":
                    grade = "BCD"; break;
                case "C":
                    grade = "CCD"; break;
                case "A":
                    grade = "ACD"; break;
                default:
                    grade = ""; break;
            }
            switch (foc)
            {
                case "1":
                    foc = "NORMAL"; break;
                case "2":
                    foc = "ODD"; break;
                case "3":
                    foc = "SHORT"; break;
                case "5":
                case "6":
                    foc = "CUTS"; break;
                default:
                    foc = ""; break;
            }
            if (grade == "G" && foc == "1")
            {
                pcstype = "FRESH";
            }
            else
            {
                pcstype = grade + " " + foc;
            }
            return pcstype;
        }

        private string getSLCD(string sapcode)
        {
            sql = "select slcd from " + CommVar.CurSchema(UNQSNO) + ".m_subleg_com where sapcode='" + sapcode + "'";
            var dt = masterHelp.SQLquery(sql);
            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0]["slcd"].ToString();
            }
            return "";
        }
    }
}
