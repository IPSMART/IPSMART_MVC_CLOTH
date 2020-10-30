﻿using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using System.IO;

namespace Improvar.Controllers
{
    public class T_SALE_POSController : Controller
    {
        // GET: T_SALE_POS_POS
        Connection Cn = new Connection(); MasterHelp masterHelp = new MasterHelp();
        Salesfunc salesfunc = new Salesfunc(); DataTable DTNEW = new DataTable();
        EmailControl EmailControl = new EmailControl();
        T_TXN TXN; T_TXNTRANS TXNTRN; T_TXNOTH TXNOTH; T_CNTRL_HDR TCH; T_CNTRL_HDR_REM SLR; T_TXN_LINKNO TTXNLINKNO;
        SMS SMS = new SMS(); T_TXNMEMO TXNMEMO;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: T_SALE_POS
        public ActionResult T_SALE_POS(string op = "", string key = "", int Nindex = 0, string searchValue = "", string parkID = "", string ThirdParty = "no", string loadOrder = "N")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Cash Sale (Pos)";
                    TransactionSalePosEntry VE = (parkID == "") ? new TransactionSalePosEntry() : (Improvar.ViewModels.TransactionSalePosEntry)Session[parkID];
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);

                    switch (VE.MENU_PARA)
                    {
                        case "SBCM":
                            ViewBag.formname = "Cash Memo"; break;
                        case "SBCMR":
                            ViewBag.formname = "Cash Memo Credit Note"; break;
                        default: ViewBag.formname = ""; break;
                    }
                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    string YR1 = CommVar.YearCode(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    VE.DropDown_list_StkType = masterHelp.STK_TYPE();
                    //VE.DISC_TYPE = masterHelp.DISC_TYPE();
                    VE.BARGEN_TYPE = masterHelp.BARGEN_TYPE();
                    VE.INVTYPE_list = masterHelp.INVTYPE_list();
                    //VE.EXPCD_list = masterHelp.EXPCD_list(VE.MENU_PARA == "PB" ? "P" : "S");
                    VE.Reverse_Charge = masterHelp.REV_CHRG();
                    VE.Database_Combo1 = (from n in DB.T_TXNOTH
                                          select new Database_Combo1() { FIELD_VALUE = n.SELBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                    VE.Database_Combo2 = (from n in DB.T_TXNOTH
                                          select new Database_Combo2() { FIELD_VALUE = n.DEALBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                    VE.Database_Combo3 = (from n in DB.T_TXNOTH
                                          select new Database_Combo3() { FIELD_VALUE = n.PACKBY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                    VE.HSN_CODE = (from n in DBF.M_HSNCODE
                                   select new HSN_CODE() { text = n.HSNDESCN, value = n.HSNCODE }).OrderBy(s => s.text).Distinct().ToList();




                    string[] autoEntryWork = ThirdParty.Split('~');// for zooming
                    if (autoEntryWork[0] == "yes")
                    {
                        autoEntryWork[2] = autoEntryWork[2].Replace("$$$$$$$$", "&");
                    }
                    if (autoEntryWork[0] == "yes")
                    {
                        if (autoEntryWork[4] == "Y")
                        {
                            DocumentType dp = new DocumentType();
                            dp.text = autoEntryWork[2];
                            dp.value = autoEntryWork[1];
                            VE.DocumentType.Clear();
                            VE.DocumentType.Add(dp);
                            VE.Edit = "E";
                            VE.Delete = "D";
                        }
                    }
                    if (op.Length != 0)
                    {
                        string[] XYZ = VE.DocumentType.Select(i => i.value).ToArray();

                        VE.IndexKey = (from p in DB.T_TXN
                                       join q in DB.T_CNTRL_HDR on p.AUTONO equals (q.AUTONO)
                                       orderby p.DOCDT, p.DOCNO
                                       where XYZ.Contains(p.DOCCD) && q.LOCCD == LOC && q.COMPCD == COM && q.YR_CD == YR1
                                       select new IndexKey() { Navikey = p.AUTONO }).ToList();
                        if (searchValue != "") { Nindex = VE.IndexKey.FindIndex(r => r.Navikey.Equals(searchValue)); }
                        if (op == "E" || op == "D" || op == "V" || loadOrder.retStr().Length > 1)
                        {
                            if (searchValue.Length != 0)
                            {
                                if (searchValue != "") { Nindex = VE.IndexKey.FindIndex(r => r.Navikey.Equals(searchValue)); }
                                VE.Index = Nindex;
                                VE = Navigation(VE, DB, 0, searchValue, loadOrder);
                            }
                            else
                            {
                                if (key == "F")
                                {
                                    VE.Index = 0;
                                    VE = Navigation(VE, DB, 0, searchValue, loadOrder);
                                }
                                else if (key == "" || key == "L")
                                {
                                    VE.Index = VE.IndexKey.Count - 1;
                                    VE = Navigation(VE, DB, VE.IndexKey.Count - 1, searchValue, loadOrder);
                                }
                                else if (key == "P")
                                {
                                    Nindex -= 1;
                                    if (Nindex < 0)
                                    {
                                        Nindex = 0;
                                    }
                                    VE.Index = Nindex;
                                    VE = Navigation(VE, DB, Nindex, searchValue, loadOrder);
                                }
                                else if (key == "N")
                                {
                                    Nindex += 1;
                                    if (Nindex > VE.IndexKey.Count - 1)
                                    {
                                        Nindex = VE.IndexKey.Count - 1;
                                    }
                                    VE.Index = Nindex;
                                    VE = Navigation(VE, DB, Nindex, searchValue, loadOrder);
                                }
                            }
                            VE.T_TXN = TXN;
                            VE.T_TXNTRANS = TXNTRN;
                            VE.T_TXNOTH = TXNOTH;
                            VE.T_TXNMEMO = TXNMEMO;
                            VE.T_CNTRL_HDR = TCH;
                            VE.T_CNTRL_HDR_REM = SLR;
                            VE.T_TXN_LINKNO = TTXNLINKNO;
                            if (loadOrder.retStr().Length > 1)
                            {
                                VE.T_TXN.AUTONO = "";
                                if (VE.T_TXN_LINKNO == null) VE.T_TXN_LINKNO = new T_TXN_LINKNO();
                                VE.LINKDOCNO = loadOrder.retStr();
                                VE.T_TXN_LINKNO.LINKAUTONO = searchValue.retStr();
                            }
                            if (VE.T_CNTRL_HDR.DOCNO != null) ViewBag.formname = ViewBag.formname + " (" + VE.T_CNTRL_HDR.DOCNO + ")";
                        }
                        if (op.ToString() == "A" && loadOrder == "N")
                        {
                            if (parkID == "")
                            {
                                T_TXN TTXN = new T_TXN();
                                TTXN.DOCDT = Cn.getCurrentDate(VE.mindate);
                                TTXN.GOCD = TempData["LASTGOCD" + VE.MENU_PARA].retStr();
                                TempData.Keep();
                                if (TTXN.GOCD.retStr() == "")
                                {
                                    if (VE.DocumentType.Count() > 0)
                                    {
                                        string doccd = VE.DocumentType.FirstOrDefault().value;
                                        TTXN.GOCD = DB.T_TXN.Where(a => a.DOCCD == doccd).Select(b => b.GOCD).FirstOrDefault();
                                    }
                                }
                                string gocd = TTXN.GOCD.retStr();

                                if (gocd != "")
                                {
                                    VE.GONM = DB.M_GODOWN.Where(a => a.GOCD == gocd).Select(b => b.GONM).FirstOrDefault();
                                }
                                VE.T_TXN = TTXN;

                                T_TXNOTH TXNOTH = new T_TXNOTH(); VE.T_TXNOTH = TXNOTH; T_TXNMEMO TXNMEMO = new T_TXNMEMO();
                                var autono = DB.M_SYSCNFG.Select(b => b.M_AUTONO).Max();
                                string scmf = CommVar.FinSchema(UNQSNO); string scm = CommVar.CurSchema(UNQSNO);
                                string sql = " select a.rtdebcd,b.rtdebnm,b.mobile,a.inc_rate from " + scm + ".M_SYSCNFG a," + scmf + ".M_RETDEB b where a.RTDEBCD=b.RTDEBCD(+) and a.M_AUTONO='" + autono + "' ";
                                DataTable syscnfg = masterHelp.SQLquery(sql);
                                if (syscnfg != null && syscnfg.Rows.Count > 0)
                                {
                                    TXNMEMO.RTDEBCD = syscnfg.Rows[0]["RTDEBCD"].retStr();
                                    VE.RTDEBNM = syscnfg.Rows[0]["RTDEBNM"].retStr();
                                    VE.MOBILE = syscnfg.Rows[0]["MOBILE"].retStr();
                                    VE.INC_RATE = syscnfg.Rows[0]["INC_RATE"].retStr() == "Y" ? true : false;
                                    VE.INCLRATEASK = syscnfg.Rows[0]["INC_RATE"].retStr();
                                }
                                VE.T_TXNMEMO = TXNMEMO;

                                //List<TsalePos_TBATCHDTL> TSHRTXNDTL = new List<TsalePos_TBATCHDTL>();
                                //for (int i = 0; i <= 9; i++)
                                //{
                                //    TsalePos_TBATCHDTL PROGDTL = new TsalePos_TBATCHDTL();
                                //    PROGDTL.SLNO = Convert.ToByte(i + 1);
                                //    PROGDTL.DropDown_list1 = DropDown_list1();
                                //    TSHRTXNDTL.Add(PROGDTL);
                                //    VE.TsalePos_TBATCHDTL = TSHRTXNDTL;
                                //}
                                //VE.TsalePos_TBATCHDTL = TSHRTXNDTL;

                                //List<TsalePos_TBATCHDTL> TSHRTXNDTL = new List<TsalePos_TBATCHDTL>();
                                //TsalePos_TBATCHDTL PROGDTL = new TsalePos_TBATCHDTL();
                                //PROGDTL.DropDown_list1 = DropDown_list1();
                                //TSHRTXNDTL.Add(PROGDTL);
                                //VE.TsalePos_TBATCHDTL = TSHRTXNDTL;
                                List<TTXNSLSMN> TTXNSLSMN = new List<TTXNSLSMN>();
                                for (int i = 0; i <= 2; i++)
                                {
                                    TTXNSLSMN TTXNSLM = new TTXNSLSMN();
                                    TTXNSLM.SLNO = Convert.ToByte(i + 1);
                                    TTXNSLSMN.Add(TTXNSLM);
                                    VE.TTXNSLSMN = TTXNSLSMN;
                                }
                                VE.TTXNSLSMN = TTXNSLSMN;
                                var pymtcd = DB.M_PAYMENT.Select(b => b.PYMTCD).Max();
                                if (pymtcd != null)
                                {
                                    var MPAYMENT = (from i in DB.M_PAYMENT where i.PYMTCD == pymtcd select new { i.PYMTCD, i.PYMTNM, i.GLCD }).ToList();

                                    if (MPAYMENT.Count > 0)
                                    {
                                        VE.TTXNPYMT = (from i in MPAYMENT select new TTXNPYMT { PYMTCD = i.PYMTCD, PYMTNM = i.PYMTNM, GLCD = i.GLCD }).ToList();
                                    }
                                    //else if (TTXNPAYMT.Count > 0)
                                    //{ VE.TTXNPYMT = (from i in TTXNPAYMT select new TTXNPYMT { PYMTCD = i.PYMTCD, PYMTNM = i.PYMTNM, GLCD = i.GLCD, INSTNO = i.INSTNO, PYMTREM = i.PYMTREM, CARDNO = i.CARDNO, AMT = i.AMT, INSTDT = i.INSTDT.retDateStr() }).ToList(); }
                                    for (int p = 0; p <= VE.TTXNPYMT.Count - 1; p++)
                                    {
                                        VE.TTXNPYMT[p].SLNO = Convert.ToInt16(p + 1);
                                    }
                                }
                                else
                                {
                                    int slno = 0;
                                    List<TTXNPYMT> TTXNPYMNT = new List<TTXNPYMT>();
                                    TTXNPYMT TXNPYMT = new TTXNPYMT();
                                    TXNPYMT.SLNO = Convert.ToByte(slno + 1);
                                    TTXNPYMNT.Add(TXNPYMT);
                                    VE.TTXNPYMT = TTXNPYMNT;
                                }

                                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                                UploadDOC UPL = new UploadDOC();
                                UPL.DocumentType = Cn.DOC_TYPE();
                                UploadDOC1.Add(UPL);
                                VE.UploadDOC = UploadDOC1;

                                List<PACKING_SLIP> PACKING_SLIP = new List<PACKING_SLIP>();
                                for (int i = 0; i < 10; i++)
                                {
                                    PACKING_SLIP PACKINGSLIP = new PACKING_SLIP();
                                    PACKINGSLIP.SLNO = Convert.ToInt16(i + 1);
                                    PACKING_SLIP.Add(PACKINGSLIP);
                                }
                            }
                            else
                            {
                                if (VE.UploadDOC != null) { foreach (var i in VE.UploadDOC) { i.DocumentType = Cn.DOC_TYPE(); } }
                                INI INIF = new INI();
                                INIF.DeleteKey(Session["UR_ID"].ToString(), parkID, Server.MapPath("~/Park.ini"));
                            }
                            VE = (TransactionSalePosEntry)Cn.CheckPark(VE, VE.MENU_DETAILS, LOC, COM, CommVar.CurSchema(UNQSNO), Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString());
                        }
                        if (parkID == "" && loadOrder == "N")
                        {
                            FreightCharges(VE, VE.T_TXN.AUTONO);
                        }
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                    }
                    string docdt = "";
                    if (TCH != null) if (TCH.DOCDT != null) docdt = TCH.DOCDT.ToString().Remove(10);
                    Cn.getdocmaxmindate(VE.T_TXN.DOCCD, docdt, VE.DefaultAction, VE.T_TXN.DOCNO, VE);
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                TransactionSalePosEntry VE = new TransactionSalePosEntry();
                Cn.SaveException(ex, "");
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                return View(VE);
            }
        }
        public TransactionSalePosEntry Navigation(TransactionSalePosEntry VE, ImprovarDB DB, int index, string searchValue, string loadOrder = "N")
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            string COM_CD = CommVar.Compcd(UNQSNO);
            string LOC_CD = CommVar.Loccd(UNQSNO);
            string DATABASE = CommVar.CurSchema(UNQSNO).ToString();
            string DATABASEF = CommVar.FinSchema(UNQSNO);
            Cn.getQueryString(VE);

            TXN = new T_TXN(); TXNTRN = new T_TXNTRANS(); TXNOTH = new T_TXNOTH(); TCH = new T_CNTRL_HDR(); SLR = new T_CNTRL_HDR_REM(); TTXNLINKNO = new T_TXN_LINKNO();
            TXNMEMO = new T_TXNMEMO();

            if (VE.IndexKey.Count != 0 || loadOrder.retStr().Length > 1)
            {
                string[] aa = null;
                if (searchValue.Length == 0)
                {
                    aa = VE.IndexKey[index].Navikey.Split(Convert.ToChar(Cn.GCS()));
                }
                else
                {
                    aa = searchValue.Split(Convert.ToChar(Cn.GCS()));
                }
                TXN = DB.T_TXN.Find(aa[0].Trim());
                TCH = DB.T_CNTRL_HDR.Find(TXN.AUTONO);
                if (TXN.ROYN == "Y") VE.RoundOff = true;
                else VE.RoundOff = false;
                TXNTRN = DB.T_TXNTRANS.Find(TXN.AUTONO);
                TXNOTH = DB.T_TXNOTH.Find(TXN.AUTONO);
                //var MGP = DB.M_GROUP.Find(TXN.ITGRPCD);
                if (VE.MENU_PARA == "SB" && loadOrder == "N")
                {
                    TTXNLINKNO = (from a in DB.T_TXN_LINKNO where a.AUTONO == TXN.AUTONO select a).FirstOrDefault();
                    VE.LINKDOCNO = (from a in DB.T_CNTRL_HDR where a.AUTONO == TTXNLINKNO.LINKAUTONO select a).FirstOrDefault().DOCNO;
                }

                VE.CONSLNM = TXN.CONSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXN.CONSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.AGSLNM = TXNOTH.AGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNOTH.AGSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.SAGSLNM = TXNOTH.SAGSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNOTH.SAGSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.GONM = TXN.GOCD.retStr() == "" ? "" : DB.M_GODOWN.Where(a => a.GOCD == TXN.GOCD).Select(b => b.GONM).FirstOrDefault();
                VE.PRCNM = TXNOTH.PRCCD.retStr() == "" ? "" : DBF.M_PRCLST.Where(a => a.PRCCD == TXNOTH.PRCCD).Select(b => b.PRCNM).FirstOrDefault();

                VE.TRANSLNM = TXNTRN.TRANSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNTRN.TRANSLCD).Select(b => b.SLNM).FirstOrDefault();
                VE.CRSLNM = TXNTRN.CRSLCD.retStr() == "" ? "" : DBF.M_SUBLEG.Where(a => a.SLCD == TXNTRN.CRSLCD).Select(b => b.SLNM).FirstOrDefault();
                SLR = Cn.GetTransactionReamrks(CommVar.CurSchema(UNQSNO).ToString(), TXN.AUTONO);
                VE.UploadDOC = Cn.GetUploadImageTransaction(CommVar.CurSchema(UNQSNO).ToString(), TXN.AUTONO);

                // tcsdata
                var tdsdt = getTDS(TXN.DOCDT.retStr().Remove(10), TXN.SLCD, TXN.TDSCODE);
                if (tdsdt != null && tdsdt.Rows.Count > 0)
                {
                    VE.TDSLIMIT = tdsdt.Rows[0]["TDSLIMIT"].retDbl();
                    VE.TDSCALCON = tdsdt.Rows[0]["TDSCALCON"].retStr();
                    VE.TDSROUNDCAL = tdsdt.Rows[0]["TDSROUNDCAL"].retStr();
                }
                var TTXNPAYMT = (from i in DB.T_TXNPYMT join j in DB.M_PAYMENT on i.PYMTCD equals j.PYMTCD where i.AUTONO == TXN.AUTONO select new { i.PYMTCD, i.PYMTREM, i.INSTNO, i.GLCD, i.CARDNO, i.AUTONO, i.AMT, i.INSTDT, j.PYMTNM, i.SLNO }).ToList();
                if (TTXNPAYMT.Count > 0)
                { VE.TTXNPYMT = (from i in TTXNPAYMT select new TTXNPYMT { SLNO = i.SLNO, PYMTCD = i.PYMTCD, PYMTNM = i.PYMTNM, GLCD = i.GLCD, INSTNO = i.INSTNO, PYMTREM = i.PYMTREM, CARDNO = i.CARDNO, AMT = i.AMT, INSTDT = i.INSTDT.retDateStr() }).ToList(); }
                string Scm = CommVar.CurSchema(UNQSNO);
                string str1 = "";
                str1 += "select i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM,p.PRTBARCODE,j.STKTYPE,q.STKNAME,i.BARNO, ";
                str1 += "j.COLRCD,m.COLRNM,m.CLRBARCODE,j.SIZECD,l.SIZENM,l.SZBARCODE,i.SHADE,i.QNTY,i.NOS,i.RATE,i.DISCRATE,i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,i.FLAGMTR,i.LOCABIN,i.BALEYR ";
                str1 += ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD,j.WPRATE,j.RPRATE,i.ITREM ";
                str1 += "from " + Scm + ".T_BATCHDTL i, " + Scm + ".T_BATCHMST j, " + Scm + ".M_SITEM k, " + Scm + ".M_SIZE l, " + Scm + ".M_COLOR m, ";
                str1 += Scm + ".M_GROUP n," + Scm + ".M_MTRLJOBMST o," + Scm + ".M_PARTS p," + Scm + ".M_STKTYPE q ";
                str1 += "where i.BARNO = j.BARNO(+) and j.ITCD = k.ITCD(+) and j.SIZECD = l.SIZECD(+) and j.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) ";
                str1 += "and i.MTRLJOBCD=o.MTRLJOBCD(+) and i.PARTCD=p.PARTCD(+) and j.STKTYPE=q.STKTYPE(+) ";
                str1 += "and i.AUTONO = '" + TXN.AUTONO + "' ";
                str1 += "order by i.SLNO ";
                DataTable tbl = masterHelp.SQLquery(str1);

                VE.TsalePos_TBATCHDTL = (from DataRow dr in tbl.Rows
                                         select new TsalePos_TBATCHDTL()
                                         {
                                             SLNO = dr["SLNO"].retShort(),
                                             TXNSLNO = dr["TXNSLNO"].retShort(),
                                             ITGRPCD = dr["ITGRPCD"].retStr(),
                                             ITGRPNM = dr["ITGRPNM"].retStr(),
                                             MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                                             MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                                             MTBARCODE = dr["MTBARCODE"].retStr(),
                                             ITCD = dr["ITCD"].retStr(),
                                             ITSTYLE = dr["STYLENO"].retStr() + "" + dr["ITNM"].retStr(),
                                             UOM = dr["UOMCD"].retStr(),
                                             STYLENO = dr["STYLENO"].retStr(),
                                             PARTCD = dr["PARTCD"].retStr(),
                                             PARTNM = dr["PARTNM"].retStr(),
                                             PRTBARCODE = dr["PRTBARCODE"].retStr(),
                                             COLRCD = dr["COLRCD"].retStr(),
                                             COLRNM = dr["COLRNM"].retStr(),
                                             CLRBARCODE = dr["CLRBARCODE"].retStr(),
                                             SIZECD = dr["SIZECD"].retStr(),
                                             SIZENM = dr["SIZENM"].retStr(),
                                             SZBARCODE = dr["SZBARCODE"].retStr(),
                                             SHADE = dr["SHADE"].retStr(),
                                             QNTY = dr["QNTY"].retDbl(),
                                             NOS = dr["NOS"].retDbl(),
                                             RATE = dr["RATE"].retDbl(),
                                             DISCRATE = dr["DISCRATE"].retDbl(),
                                             DISCTYPE = dr["DISCTYPE"].retStr(),
                                             DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                             TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                                             TDDISCTYPE_DESC = dr["TDDISCTYPE"].retStr() == "P" ? "%" : dr["TDDISCTYPE"].retStr() == "N" ? "Nos" : dr["TDDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                             TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
                                             SCMDISCTYPE_DESC = dr["SCMDISCTYPE"].retStr() == "P" ? "%" : dr["SCMDISCTYPE"].retStr() == "N" ? "Nos" : dr["SCMDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                             SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
                                             SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
                                             STKTYPE = dr["STKTYPE"].retStr(),
                                             STKNAME = dr["STKNAME"].retStr(),
                                             BARNO = dr["BARNO"].retStr(),
                                             HSNCODE = dr["HSNCODE"].retStr(),
                                             BALENO = dr["BALENO"].retStr(),
                                             PDESIGN = dr["PDESIGN"].retStr(),
                                             OURDESIGN = dr["OURDESIGN"].retStr(),
                                             FLAGMTR = dr["FLAGMTR"].retDbl(),
                                             LOCABIN = dr["LOCABIN"].retStr(),
                                             BALEYR = dr["BALEYR"].retStr(),
                                             BARGENTYPE = dr["BARGENTYPE"].retStr(),
                                             GLCD = VE.MENU_PARA == "SBPCK" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SB" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SBDIR" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SR" ? dr["SALRETGLCD"].retStr() : VE.MENU_PARA == "SBCM" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SBCMR" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SBEXP" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "PI" ? "" : VE.MENU_PARA == "PB" ? dr["PURGLCD"].retStr() : VE.MENU_PARA == "PR" ? dr["PURRETGLCD"].retStr() : "",
                                             WPRATE = VE.MENU_PARA == "PB" ? dr["WPRATE"].retDbl() : (double?)null,
                                             RPRATE = VE.MENU_PARA == "PB" ? dr["RPRATE"].retDbl() : (double?)null,
                                             ITREM = dr["ITREM"].retStr(),
                                         }).OrderBy(s => s.SLNO).ToList();

                str1 = "";
                str1 += "select i.SLNO,j.ITGRPCD,k.ITGRPNM,i.MTRLJOBCD,l.MTRLJOBNM,l.MTBARCODE,i.ITCD,j.ITNM,j.STYLENO,j.UOMCD,i.STKTYPE,m.STKNAME,i.NOS,i.QNTY,i.FLAGMTR, ";
                str1 += "i.BLQNTY,i.RATE,i.AMT,i.DISCTYPE,i.DISCRATE,i.DISCAMT,i.TDDISCTYPE,i.TDDISCRATE,i.TDDISCAMT,i.SCMDISCTYPE,i.SCMDISCRATE,i.SCMDISCAMT, ";
                str1 += "i.TXBLVAL,i.IGSTPER,i.CGSTPER,i.SGSTPER,i.CESSPER,i.IGSTAMT,i.CGSTAMT,i.SGSTAMT,i.CESSAMT,i.NETAMT,i.HSNCODE,i.BALENO,i.GLCD,i.BALEYR,i.TOTDISCAMT,i.ITREM ";
                str1 += "from " + Scm + ".T_TXNDTL i, " + Scm + ".M_SITEM j, " + Scm + ".M_GROUP k, " + Scm + ".M_MTRLJOBMST l, " + Scm + ".M_STKTYPE m ";
                str1 += "where i.ITCD = j.ITCD(+) and j.ITGRPCD=k.ITGRPCD(+) and i.MTRLJOBCD=l.MTRLJOBCD(+)  and i.STKTYPE=m.STKTYPE(+)  ";
                str1 += "and i.AUTONO = '" + TXN.AUTONO + "' ";
                str1 += "order by i.SLNO ";
                tbl = masterHelp.SQLquery(str1);

                VE.TTXNDTL = (from DataRow dr in tbl.Rows
                              select new TTXNDTL()
                              {
                                  SLNO = dr["SLNO"].retShort(),
                                  ITGRPCD = dr["ITGRPCD"].retStr(),
                                  ITGRPNM = dr["ITGRPNM"].retStr(),
                                  MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                                  MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                                  //   MTBARCODE = dr["MTBARCODE"].retStr(),
                                  ITCD = dr["ITCD"].retStr(),
                                  ITSTYLE = dr["STYLENO"].retStr() + "" + dr["ITNM"].retStr(),
                                  UOM = dr["UOMCD"].retStr(),
                                  STKTYPE = dr["STKTYPE"].retStr(),
                                  STKNAME = dr["STKNAME"].retStr(),
                                  NOS = dr["NOS"].retDbl(),
                                  QNTY = dr["QNTY"].retDbl(),
                                  FLAGMTR = dr["FLAGMTR"].retDbl(),
                                  BLQNTY = dr["BLQNTY"].retDbl(),
                                  RATE = dr["RATE"].retDbl(),
                                  AMT = dr["AMT"].retDbl(),
                                  DISCTYPE = dr["DISCTYPE"].retStr(),
                                  DISCTYPE_DESC = dr["DISCTYPE"].retStr() == "P" ? "%" : dr["DISCTYPE"].retStr() == "N" ? "Nos" : dr["DISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                  DISCRATE = dr["DISCRATE"].retDbl(),
                                  DISCAMT = dr["DISCAMT"].retDbl(),
                                  TDDISCTYPE_DESC = dr["TDDISCTYPE"].retStr() == "P" ? "%" : dr["TDDISCTYPE"].retStr() == "N" ? "Nos" : dr["TDDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                  TDDISCTYPE = dr["TDDISCTYPE"].retStr(),
                                  TDDISCRATE = dr["TDDISCRATE"].retDbl(),
                                  TDDISCAMT = dr["TDDISCAMT"].retDbl(),
                                  SCMDISCTYPE_DESC = dr["SCMDISCTYPE"].retStr() == "P" ? "%" : dr["SCMDISCTYPE"].retStr() == "N" ? "Nos" : dr["SCMDISCTYPE"].retStr() == "Q" ? "Qnty" : "Fixed",
                                  SCMDISCTYPE = dr["SCMDISCTYPE"].retStr(),
                                  SCMDISCRATE = dr["SCMDISCRATE"].retDbl(),
                                  SCMDISCAMT = dr["SCMDISCAMT"].retDbl(),
                                  TXBLVAL = dr["TXBLVAL"].retDbl(),
                                  IGSTPER = dr["IGSTPER"].retDbl(),
                                  CGSTPER = dr["CGSTPER"].retDbl(),
                                  SGSTPER = dr["SGSTPER"].retDbl(),
                                  CESSPER = dr["CESSPER"].retDbl(),
                                  IGSTAMT = dr["IGSTAMT"].retDbl(),
                                  CGSTAMT = dr["CGSTAMT"].retDbl(),
                                  SGSTAMT = dr["SGSTAMT"].retDbl(),
                                  CESSAMT = dr["CESSAMT"].retDbl(),
                                  NETAMT = dr["NETAMT"].retDbl(),
                                  HSNCODE = dr["HSNCODE"].retStr(),
                                  BALENO = dr["BALENO"].retStr(),
                                  GLCD = dr["GLCD"].retStr(),
                                  BALEYR = dr["BALEYR"].retStr(),
                                  TOTDISCAMT = dr["TOTDISCAMT"].retDbl(),
                                  ITREM = dr["ITREM"].retStr(),
                              }).OrderBy(s => s.SLNO).ToList();

                VE.B_T_QNTY = VE.TsalePos_TBATCHDTL.Sum(a => a.QNTY).retDbl();
                VE.B_T_NOS = VE.TsalePos_TBATCHDTL.Sum(a => a.NOS).retDbl();
                VE.T_NOS = VE.TTXNDTL.Sum(a => a.NOS).retDbl();
                VE.T_QNTY = VE.TTXNDTL.Sum(a => a.QNTY).retDbl();
                VE.T_AMT = VE.TTXNDTL.Sum(a => a.AMT).retDbl();
                VE.T_GROSS_AMT = VE.TTXNDTL.Sum(a => a.TXBLVAL).retDbl();
                VE.T_IGST_AMT = VE.TTXNDTL.Sum(a => a.IGSTAMT).retDbl();
                VE.T_CGST_AMT = VE.TTXNDTL.Sum(a => a.CGSTAMT).retDbl();
                VE.T_SGST_AMT = VE.TTXNDTL.Sum(a => a.SGSTAMT).retDbl();
                VE.T_CESS_AMT = VE.TTXNDTL.Sum(a => a.CESSAMT).retDbl();
                VE.T_NET_AMT = VE.TTXNDTL.Sum(a => a.NETAMT).retDbl();
                if (VE.MENU_PARA == "PB" && VE.TsalePos_TBATCHDTL.Count() > 0)
                {
                    VE.WPRATE = VE.TsalePos_TBATCHDTL.Where(a => a.WPRATE.retDbl() != 0).Select(b => b.WPRATE).FirstOrDefault();
                    VE.RPRATE = VE.TsalePos_TBATCHDTL.Where(a => a.RPRATE.retDbl() != 0).Select(b => b.RPRATE).FirstOrDefault();
                }

                // fill prodgrpgstper in t_batchdtl
                DataTable allprodgrpgstper_data = new DataTable();
                string BARNO = (from a in VE.TsalePos_TBATCHDTL select a.BARNO).ToArray().retSqlfromStrarray();
                string ITCD = (from a in VE.TsalePos_TBATCHDTL select a.ITCD).ToArray().retSqlfromStrarray();
                string MTRLJOBCD = (from a in VE.TsalePos_TBATCHDTL select a.MTRLJOBCD).ToArray().retSqlfromStrarray();
                string ITGRPCD = (from a in VE.TsalePos_TBATCHDTL select a.ITGRPCD).ToArray().retSqlfromStrarray();
                if (VE.MENU_PARA == "PB")
                {
                    allprodgrpgstper_data = salesfunc.GetBarHelp(TXN.DOCDT.retStr().Remove(10), TXN.GOCD.retStr(), BARNO.retStr(), ITCD.retStr(), "", "", ITGRPCD, "", TXNOTH.PRCCD.retStr(), TXNOTH.TAXGRPCD.retStr(), "", "", true, false, VE.MENU_PARA);

                }
                else
                {
                    allprodgrpgstper_data = salesfunc.GetStock(TXN.DOCDT.retStr().Remove(10), TXN.GOCD.retSqlformat(), BARNO.retStr(), ITCD.retStr(), "", SLR.AUTONO.retSqlformat(), ITGRPCD, "", TXNOTH.PRCCD.retStr(), TXNOTH.TAXGRPCD.retStr());
                }
                foreach (var v in VE.TsalePos_TBATCHDTL)
                {
                    string PRODGRPGSTPER = "", ALL_GSTPER = "", GSTPER = "";
                    v.GSTPER = VE.TTXNDTL.Where(a => a.SLNO == v.TXNSLNO).Sum(b => b.IGSTPER + b.CGSTPER + b.SGSTPER).retDbl();
                    if (allprodgrpgstper_data != null && allprodgrpgstper_data.Rows.Count > 0)
                    {
                        var DATA = allprodgrpgstper_data.Select("barno = '" + v.BARNO + "' and itcd= '" + v.ITCD + "' and itgrpcd = '" + v.ITGRPCD + "'");
                        if (DATA.Count() > 0)
                        {
                            DataTable tax_data = DATA.CopyToDataTable();
                            if (tax_data != null && tax_data.Rows.Count > 0)
                            {
                                PRODGRPGSTPER = tax_data.Rows[0]["PRODGRPGSTPER"].retStr();
                                if (PRODGRPGSTPER != "")
                                {
                                    ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, v.RATE.retDbl());
                                    if (ALL_GSTPER.retStr() != "")
                                    {
                                        var gst = ALL_GSTPER.Split(',').ToList();
                                        GSTPER = (from a in gst select a.retDbl()).Sum().retStr();
                                    }
                                    v.PRODGRPGSTPER = PRODGRPGSTPER;
                                    v.ALL_GSTPER = ALL_GSTPER;
                                    v.GSTPER = GSTPER.retDbl();
                                }
                                if (tax_data.Rows[0]["barimage"].retStr() != "")
                                {
                                    v.BarImages = tax_data.Rows[0]["barimage"].retStr();
                                    var brimgs = v.BarImages.retStr().Split((char)179);
                                    v.BarImagesCount = brimgs.Length == 0 ? "" : brimgs.Length.retStr();
                                    foreach (var barimg in brimgs)
                                    {
                                        string barfilename = barimg.Split('~')[0];
                                        string FROMpath = CommVar.SaveFolderPath() + "/ItemImages/" + barfilename;
                                        FROMpath = Path.Combine(FROMpath, "");
                                        string TOPATH = System.Web.Hosting.HostingEnvironment.MapPath("/UploadDocuments/" + barfilename);
                                        Cn.CopyImage(FROMpath, TOPATH);
                                    }
                                }
                            }
                        }
                    }

                    //   checking childdata exist against barno
                    var chk_child = (from a in DB.T_BATCHDTL where a.BARNO == v.BARNO && a.AUTONO != TXN.AUTONO select a).ToList();
                    if (chk_child.Count() > 0)
                    {
                        v.ChildData = "Y";
                    }
                    if ((VE.MENU_PARA == "PB") && ((TXN.BARGENTYPE == "E") || (TXN.BARGENTYPE == "C" && v.BARGENTYPE == "E")))
                    {
                        v.WPRATE = v.RATE * VE.WPRATE;
                        v.RPRATE = v.RATE * VE.RPRATE;
                    }
                }
                // fill prodgrpgstper in t_txndtl
                double IGST_PER = 0; double CGST_PER = 0; double SGST_PER = 0; double CESS_PER = 0; double DUTY_PER = 0;
                foreach (var v in VE.TTXNDTL)
                {
                    string PRODGRPGSTPER = "", ALL_GSTPER = "";
                    var tax_data = (from a in VE.TsalePos_TBATCHDTL
                                    where a.TXNSLNO == v.SLNO && a.ITGRPCD == v.ITGRPCD && a.ITCD == a.ITCD && a.STKTYPE == v.STKTYPE
                                    && a.RATE == v.RATE && a.DISCTYPE == v.DISCTYPE && a.DISCRATE == v.DISCRATE && a.TDDISCTYPE == v.TDDISCTYPE
                                     && a.TDDISCRATE == v.TDDISCRATE && a.SCMDISCTYPE == v.SCMDISCTYPE && a.SCMDISCRATE == v.SCMDISCRATE
                                    select new { a.PRODGRPGSTPER, a.ALL_GSTPER }).FirstOrDefault();
                    if (tax_data != null)
                    {
                        PRODGRPGSTPER = tax_data.PRODGRPGSTPER.retStr();
                        ALL_GSTPER = tax_data.ALL_GSTPER.retStr();
                    }
                    v.PRODGRPGSTPER = PRODGRPGSTPER;
                    v.ALL_GSTPER = ALL_GSTPER;

                    double IGST = v.IGSTPER.retDbl();
                    double CGST = v.CGSTPER.retDbl();
                    double SGST = v.SGSTPER.retDbl();
                    double CESS = v.CESSPER.retDbl();
                    double DUTY = v.DUTYPER.retDbl();

                    if (IGST > IGST_PER)
                    {
                        IGST_PER = IGST;
                    }
                    if (CGST > CGST_PER)
                    {
                        CGST_PER = CGST;
                    }
                    if (SGST > SGST_PER)
                    {
                        SGST_PER = SGST;
                    }
                    if (CESS > CESS_PER)
                    {
                        CESS_PER = CESS;
                    }
                    if (DUTY > DUTY_PER)
                    {
                        DUTY_PER = DUTY;
                    }
                }
                string S_P = "";
                if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "OP") { S_P = "P"; } else { S_P = "S"; }
                string sql = "select a.amtcd, b.amtnm, b.calccode, b.addless, b.taxcode, b.calctype, b.calcformula, a.amtdesc, ";
                sql += "b.glcd, b.hsncode, a.amtrate, a.curr_amt, a.amt,a.igstper, a.igstamt, a.sgstper, a.sgstamt, ";
                sql += "a.cgstper, a.cgstamt,a.cessper, a.cessamt, a.dutyper, a.dutyamt ";
                sql += "from " + Scm + ".t_txnamt a, " + Scm + ".m_amttype b ";
                sql += "where a.amtcd=b.amtcd(+) and b.salpur='" + S_P + "' and a.autono='" + TXN.AUTONO + "' ";
                sql += "union ";
                sql += "select b.amtcd, b.amtnm, b.calccode, b.addless, b.taxcode, b.calctype, b.calcformula, '' amtdesc, ";
                sql += "b.glcd, b.hsncode, 0 amtrate, 0 curr_amt, 0 amt,0 igstper, 0 igstamt, 0 sgstper, 0 sgstamt, ";
                sql += "0 cgstper, 0 cgstamt, 0 CESSPER, 0 CESSAMT, 0 dutyper, 0 dutyamt ";
                sql += "from " + Scm + ".m_amttype b, " + Scm + ".m_cntrl_hdr c ";
                sql += "where b.m_autono=c.m_autono(+) and b.salpur='" + S_P + "'  and nvl(c.inactive_tag,'N') = 'N' and ";
                sql += "b.amtcd not in (select amtcd from " + Scm + ".t_txnamt where autono='" + TXN.AUTONO + "') ";
                var AMOUNT_DATA = masterHelp.SQLquery(sql);

                VE.TTXNAMT = (from DataRow dr in AMOUNT_DATA.Rows
                              select new TTXNAMT()
                              {
                                  AMTCD = dr["amtcd"].ToString(),
                                  ADDLESS = dr["addless"].ToString(),
                                  TAXCODE = dr["taxcode"].ToString(),
                                  GLCD = dr["GLCD"].ToString(),
                                  AMTNM = dr["amtnm"].ToString(),
                                  CALCCODE = dr["calccode"].ToString(),
                                  CALCTYPE = dr["CALCTYPE"].ToString(),
                                  CALCFORMULA = dr["calcformula"].ToString(),
                                  AMTDESC = dr["amtdesc"].ToString(),
                                  HSNCODE = dr["hsncode"].ToString(),
                                  AMTRATE = Convert.ToDouble(dr["amtrate"] == DBNull.Value ? null : dr["amtrate"].ToString()),
                                  CURR_AMT = Convert.ToDouble(dr["curr_amt"] == DBNull.Value ? null : dr["curr_amt"].ToString()),
                                  AMT = Convert.ToDouble(dr["amt"] == DBNull.Value ? null : dr["amt"].ToString()),
                                  IGSTPER = Convert.ToDouble(dr["igstper"] == DBNull.Value ? null : dr["igstper"].ToString()),
                                  IGSTPER_DESP = Convert.ToDouble(dr["igstper"] == DBNull.Value ? null : dr["igstper"].ToString()),
                                  CGSTPER = Convert.ToDouble(dr["cgstper"] == DBNull.Value ? null : dr["cgstper"].ToString()),
                                  CGSTPER_DESP = Convert.ToDouble(dr["cgstper"] == DBNull.Value ? null : dr["cgstper"].ToString()),
                                  SGSTPER = Convert.ToDouble(dr["sgstper"] == DBNull.Value ? null : dr["sgstper"].ToString()),
                                  SGSTPER_DESP = Convert.ToDouble(dr["sgstper"] == DBNull.Value ? null : dr["sgstper"].ToString()),
                                  CESSPER = Convert.ToDouble(dr["CESSPER"] == DBNull.Value ? null : dr["CESSPER"].ToString()),
                                  CESSPER_DESP = Convert.ToDouble(dr["CESSPER"] == DBNull.Value ? null : dr["CESSPER"].ToString()),
                                  DUTYPER = Convert.ToDouble(dr["dutyper"] == DBNull.Value ? null : dr["dutyper"].ToString()),
                                  IGSTAMT = Convert.ToDouble(dr["igstamt"] == DBNull.Value ? null : dr["igstamt"].ToString()),
                                  IGSTAMT_DESP = Convert.ToDouble(dr["igstamt"] == DBNull.Value ? null : dr["igstamt"].ToString()),
                                  CGSTAMT = Convert.ToDouble(dr["cgstamt"] == DBNull.Value ? null : dr["cgstamt"].ToString()),
                                  CGSTAMT_DESP = Convert.ToDouble(dr["cgstamt"] == DBNull.Value ? null : dr["cgstamt"].ToString()),
                                  SGSTAMT = Convert.ToDouble(dr["sgstamt"] == DBNull.Value ? null : dr["sgstamt"].ToString()),
                                  SGSTAMT_DESP = Convert.ToDouble(dr["sgstamt"] == DBNull.Value ? null : dr["sgstamt"].ToString()),
                                  CESSAMT = Convert.ToDouble(dr["CESSAMT"] == DBNull.Value ? null : dr["CESSAMT"].ToString()),
                                  CESSAMT_DESP = Convert.ToDouble(dr["CESSAMT"] == DBNull.Value ? null : dr["CESSAMT"].ToString()),
                                  DUTYAMT = Convert.ToDouble(dr["dutyamt"] == DBNull.Value ? null : dr["dutyamt"].ToString()),
                              }).ToList();

                double A_IGST_AMT = 0; double A_CGST_AMT = 0; double A_SGST_AMT = 0; double A_CESS_AMT = 0; double A_DUTY_AMT = 0; double A_TOTAL_CURR = 0; double A_TOTAL_AMOUNT = 0; double A_TOTAL_NETAMOUNT = 0;

                for (int p = 0; p <= VE.TTXNAMT.Count - 1; p++)
                {
                    VE.TTXNAMT[p].SLNO = Convert.ToInt16(p + 1);

                    if (VE.TTXNAMT[p].IGSTPER == 0) { VE.TTXNAMT[p].IGSTPER = IGST_PER; }
                    if (VE.TTXNAMT[p].IGSTPER_DESP == 0) { VE.TTXNAMT[p].IGSTPER_DESP = IGST_PER; }
                    if (VE.TTXNAMT[p].CGSTPER == 0) { VE.TTXNAMT[p].CGSTPER = CGST_PER; }
                    if (VE.TTXNAMT[p].CGSTPER_DESP == 0) { VE.TTXNAMT[p].CGSTPER_DESP = CGST_PER; }
                    if (VE.TTXNAMT[p].SGSTPER == 0) { VE.TTXNAMT[p].SGSTPER = SGST_PER; }
                    if (VE.TTXNAMT[p].SGSTPER_DESP == 0) { VE.TTXNAMT[p].SGSTPER_DESP = SGST_PER; }
                    if (VE.TTXNAMT[p].CESSPER == 0) { VE.TTXNAMT[p].CESSPER = CESS_PER; }
                    if (VE.TTXNAMT[p].CESSPER_DESP == 0) { VE.TTXNAMT[p].CESSPER_DESP = CESS_PER; }
                    if (VE.TTXNAMT[p].DUTYPER == 0) { VE.TTXNAMT[p].DUTYPER = DUTY_PER; }

                    A_TOTAL_CURR = A_TOTAL_CURR + VE.TTXNAMT[p].CURR_AMT.Value;
                    A_TOTAL_AMOUNT = A_TOTAL_AMOUNT + VE.TTXNAMT[p].AMT.Value;
                    A_IGST_AMT = A_IGST_AMT + VE.TTXNAMT[p].IGSTAMT.Value;
                    A_CGST_AMT = A_CGST_AMT + VE.TTXNAMT[p].CGSTAMT.Value;
                    A_SGST_AMT = A_SGST_AMT + VE.TTXNAMT[p].SGSTAMT.Value;
                    A_CESS_AMT = A_CESS_AMT + VE.TTXNAMT[p].CESSAMT.Value;
                    A_DUTY_AMT = A_DUTY_AMT + VE.TTXNAMT[p].DUTYAMT.Value;
                    VE.TTXNAMT[p].NETAMT = VE.TTXNAMT[p].AMT.Value + VE.TTXNAMT[p].IGSTAMT.Value + VE.TTXNAMT[p].CGSTAMT.Value + VE.TTXNAMT[p].SGSTAMT.Value + VE.TTXNAMT[p].CESSAMT.Value + VE.TTXNAMT[p].DUTYAMT.Value;
                    A_TOTAL_NETAMOUNT = A_TOTAL_NETAMOUNT + VE.TTXNAMT[p].NETAMT.Value;
                }
                VE.A_T_CURR = A_TOTAL_CURR;
                VE.A_T_AMOUNT = A_TOTAL_AMOUNT;
                VE.A_T_NET = A_TOTAL_NETAMOUNT;
                VE.A_T_IGST = A_IGST_AMT;
                VE.A_T_CGST = A_CGST_AMT;
                VE.A_T_SGST = A_SGST_AMT;
                VE.A_T_CESS = A_CESS_AMT;
                VE.A_T_DUTY = A_DUTY_AMT;

                // total amt
                VE.TOTNOS = VE.T_NOS.retDbl();
                VE.TOTQNTY = VE.T_QNTY.retDbl();
                VE.TOTTAXVAL = VE.T_GROSS_AMT.retDbl() + VE.A_T_AMOUNT.retDbl();
                VE.TOTTAX = VE.T_CGST_AMT.retDbl() + VE.A_T_CGST.retDbl() + VE.T_SGST_AMT.retDbl() + VE.A_T_SGST.retDbl() + VE.T_IGST_AMT.retDbl() + VE.A_T_IGST.retDbl();


            }
            //Cn.DateLock_Entry(VE, DB, TCH.DOCDT.Value);
            if (TCH.CANCEL == "Y") VE.CancelRecord = true; else VE.CancelRecord = false;
            return VE;
        }
        public ActionResult SearchPannelData(TransactionSalePosEntry VE, string SRC_SLCD, string SRC_DOCNO, string SRC_FDT, string SRC_TDT)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), yrcd = CommVar.YearCode(UNQSNO);
                Cn.getQueryString(VE);
                List<DocumentType> DocumentType = new List<DocumentType>();
                DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                string doccd = DocumentType.Select(i => i.value).ToArray().retSqlfromStrarray();
                string sql = "";

                sql = "select a.autono, b.docno, to_char(b.docdt,'dd/mm/yyyy') docdt, b.doccd, a.slcd, c.slnm, c.district, nvl(a.blamt,0) blamt ";
                sql += "from " + scm + ".t_txn a, " + scm + ".t_cntrl_hdr b, " + scmf + ".m_subleg c ";
                sql += "where a.autono=b.autono and a.slcd=c.slcd(+) and b.doccd in (" + doccd + ") and ";
                if (SRC_FDT.retStr() != "") sql += "b.docdt >= to_date('" + SRC_FDT.retDateStr() + "','dd/mm/yyyy') and ";
                if (SRC_TDT.retStr() != "") sql += "b.docdt <= to_date('" + SRC_TDT.retDateStr() + "','dd/mm/yyyy') and ";
                if (SRC_DOCNO.retStr() != "") sql += "(b.vchrno like '%" + SRC_DOCNO.retStr() + "%' or b.docno like '%" + SRC_DOCNO.retStr() + "%') and ";
                if (SRC_SLCD.retStr() != "") sql += "(a.slcd like '%" + SRC_SLCD.retStr() + "%' or upper(c.slnm) like '%" + SRC_SLCD.retStr().ToUpper() + "%') and ";
                sql += "b.loccd='" + LOC + "' and b.compcd='" + COM + "' and b.yr_cd='" + yrcd + "' ";
                sql += "order by docdt, docno ";
                DataTable tbl = masterHelp.SQLquery(sql);

                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                var hdr = "Document Number" + Cn.GCS() + "Document Date" + Cn.GCS() + "Party Name" + Cn.GCS() + "Bill Amt" + Cn.GCS() + "AUTO NO";
                for (int j = 0; j <= tbl.Rows.Count - 1; j++)
                {
                    SB.Append("<tr><td><b>" + tbl.Rows[j]["docno"] + "</b> [" + tbl.Rows[j]["doccd"] + "]" + " </td><td>" + tbl.Rows[j]["docdt"] + " </td><td><b>" + tbl.Rows[j]["slnm"] + "</b> [" + tbl.Rows[j]["district"] + "] (" + tbl.Rows[j]["slcd"] + ") </td><td class='text-right'>" + Convert.ToDouble(tbl.Rows[j]["blamt"]).ToINRFormat() + " </td><td>" + tbl.Rows[j]["autono"] + " </td></tr>");
                }
                return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "4", "4"));
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public List<DropDown_list1> DropDown_list1()
        {
            List<DropDown_list1> DropDown_list1 = new List<DropDown_list1> {
                new DropDown_list1 { value = "", text = ""},
                new DropDown_list1 { value = "DL", text = "Delivered"},
                 new DropDown_list1 { value = "FP", text = "Fall"},
                  new DropDown_list1 { value = "PO", text = "Polish"},
                   new DropDown_list1 { value = "PP", text = "Polish/Fall"},
                    new DropDown_list1 { value = "AL", text = "Alteration"},

            };
            return (DropDown_list1);
        }
        private string TranBarcodeGenerate(string doccd, string lbatchini, string docbarcode, string UNIQNO, int slno)
        {//YRCODE	2,lbatchini	2,TXN UNIQ NO	7,SLNO	4
            var yrcd = CommVar.YearCode(UNQSNO).Substring(2, 2);
            return yrcd + lbatchini + UNIQNO + slno.ToString().PadLeft(4, '0');
        }
        public ActionResult GetTDSDetails(string val, string TAG, string PARTY)
        {
            try
            {
                string linktdscode = "'Y','Z'", menu_para = "SB";
                if (menu_para == "PB" || menu_para == "PR") linktdscode = "'X'";
                if (TAG.retStr() == "") return Content("Enter Document Date");
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.TDSCODE_help(TAG, val, PARTY, "TCS", linktdscode));
                }
                else
                {
                    return Content(masterHelp.TDSCODE_help(TAG, val, PARTY, "TCS", linktdscode));
                }
            }
            catch (Exception Ex)
            {
                Cn.SaveException(Ex, "");
                return Content(Ex.Message + Ex.InnerException);
            }
        }
        public DataTable getTDS(string docdt, string slcd, string linktdscode = "")
        {
            string scmf = CommVar.FinSchema(UNQSNO);
            string sql = "select a.tdscode, a.edate, a.tdsper, a.tdspernoncmp, ";
            if (slcd.retStr() != "") sql += "(select CMPNONCMP from  " + scmf + ".m_subleg where slcd='" + slcd + "') as CMPNONCMP, "; else sql += "'' CMPNONCMP, ";
            sql += " b.tdsnm, b.secno, b.glcd,a.tdslimit,a.tdscalcon,a.tdsroundcal from ";
            sql += "(select tdscode, edate, tdsper, tdspernoncmp,tdslimit,tdscalcon,tdsroundcal from ";
            sql += "(select a.tdscode, a.edate, a.tdsper, a.tdspernoncmp,a.tdslimit,a.tdscalcon,a.tdsroundcal, ";
            sql += "row_number() over(partition by a.tdscode order by a.edate desc) as rn ";
            sql += "from " + scmf + ".m_tds_cntrl_dtl a ";
            sql += "where  edate <= to_date('" + docdt + "', 'dd/mm/yyyy')  ";
            if (linktdscode.retStr() != "") sql += " and tdscode in (" + linktdscode + ") ";
            sql += ") where rn = 1 ) a, ";
            sql += "" + scmf + ".m_tds_cntrl b ";
            sql += "where a.tdscode = b.tdscode(+) ";

            DataTable dt = masterHelp.SQLquery(sql);
            return dt;
        }
        public ActionResult GetSubLedgerDetails(string val, string Code)
        {
            try
            {
                var str = masterHelp.SLCD_help(val, Code);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetPaymentDetails(string val)
        {
            try
            {
                var str = masterHelp.PAYMTCD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetRefRetailDetails(string val)
        {
            try
            {
                var str = masterHelp.RTDEBCD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetPriceDetails(string val)
        {
            try
            {
                var str = masterHelp.PRCCD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetItemGroupDetails(string val, string Code)
        {
            try
            {
                TransactionSalePosEntry VE = new TransactionSalePosEntry();
                Cn.getQueryString(VE);
                string str = masterHelp.ITGRPCD_help(val, "", Code);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    string glcd = "";
                    switch (VE.MENU_PARA)
                    {
                        case "SBPCK"://Packing Slip
                            glcd = str.retCompValue("SALGLCD"); break;
                        case "SB"://Sales Bill (Agst Packing Slip)
                            glcd = str.retCompValue("SALGLCD"); break;
                        case "SBDIR"://Sales Bill
                            glcd = str.retCompValue("SALGLCD"); break;
                        case "SR"://Sales Return (SRM)
                            glcd = str.retCompValue("SALRETGLCD"); break;
                        case "SBCM"://Cash Memo
                            glcd = str.retCompValue("SALGLCD"); break;
                        case "SBCMR"://Cash Memo Return Note
                            glcd = str.retCompValue("SALGLCD"); break;
                        case "SBEXP"://Sales Bill (Export)
                            glcd = str.retCompValue("SALGLCD"); break;
                        case "PI"://Proforma Invoice
                            glcd = ""; break;
                        case "PB"://Purchase Bill
                            glcd = str.retCompValue("PURGLCD"); break;
                        case "PR"://Purchase Return (PRM)
                            glcd = str.retCompValue("PURRETGLCD"); break;
                        default: glcd = ""; break;
                    }
                    str += "^GLCD=^" + glcd + Cn.GCS();
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetMaterialJobDetails(string val)
        {
            try
            {
                string str = masterHelp.MTRLJOBCD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetItemDetails(string val, string Code)
        {
            try
            {
                TransactionSalePosEntry VE = new TransactionSalePosEntry();
                Cn.getQueryString(VE);
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                string ITGRPCD = data[0].retStr() == "" ? "" : data[0].retStr().retSqlformat();
                string DOCDT = data[1].retStr();
                if (DOCDT == "")
                {
                    return Content("Please Select Document Date");
                }
                string TAXGRPCD = data[2].retStr();
                string GOCD = data[3].retStr() == "" ? "" : data[3].retStr().retSqlformat();
                string PRCCD = data[4].retStr();
                string MTRLJOBCD = data[5].retStr() == "" ? "" : data[5].retStr().retSqlformat();
                string BARNO = data[6].retStr() == "" ? "" : data[6].retStr().retSqlformat();
                double RATE = data[7].retDbl();
                var str = masterHelp.ITCD_help(val, "", data[0].retStr());
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else if (str.IndexOf(Convert.ToChar(Cn.GCS())) >= 0)
                {
                    string PRODGRPGSTPER = "", ALL_GSTPER = "", GSTPER = "", BARIMAGE = "";
                    DataTable tax_data = new DataTable();
                    if (VE.MENU_PARA == "PB")
                    {
                        tax_data = salesfunc.GetBarHelp(DOCDT.retStr(), GOCD.retStr(), BARNO.retStr(), val.retStr().retSqlformat(), MTRLJOBCD.retStr(), "", ITGRPCD, "", PRCCD.retStr(), TAXGRPCD.retStr(), "", "", true, false, VE.MENU_PARA);

                    }
                    else
                    {
                        tax_data = salesfunc.GetStock(DOCDT.retStr(), GOCD.retStr(), BARNO.retStr(), val.retStr().retSqlformat(), MTRLJOBCD.retStr(), "", ITGRPCD, "", PRCCD.retStr(), TAXGRPCD.retStr());

                    }
                    if (tax_data != null && tax_data.Rows.Count > 0)
                    {
                        PRODGRPGSTPER = tax_data.Rows[0]["PRODGRPGSTPER"].retStr();
                        BARIMAGE = tax_data.Rows[0]["BARIMAGE"].retStr();
                        if (PRODGRPGSTPER != "")
                        {
                            ALL_GSTPER = salesfunc.retGstPer(PRODGRPGSTPER, RATE);
                            if (ALL_GSTPER.retStr() != "")
                            {
                                var gst = ALL_GSTPER.Split(',').ToList();
                                GSTPER = (from a in gst select a.retDbl()).Sum().retStr();
                            }
                        }
                    }
                    str += "^PRODGRPGSTPER=^" + PRODGRPGSTPER + Cn.GCS();
                    str += "^BARIMAGE=^" + BARIMAGE + Cn.GCS();
                    str += "^ALL_GSTPER=^" + ALL_GSTPER + Cn.GCS();
                    str += "^GSTPER=^" + GSTPER + Cn.GCS();
                    return Content(str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetPartDetails(string val)
        {
            try
            {
                var str = masterHelp.PARTCD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetColorDetails(string val)
        {
            try
            {
                var str = masterHelp.COLRCD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetSizeDetails(string val)
        {
            try
            {
                var str = masterHelp.SIZECD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetStockDetails(string val)
        {
            try
            {
                var str = masterHelp.STKTYPE_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetLocationBinDetails(string val)
        {
            try
            {
                var str = masterHelp.LOCABIN_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetBarCodeDetails(string val, string Code)
        {
            try
            {
                TransactionSalePosEntry VE = new TransactionSalePosEntry();
                Cn.getQueryString(VE);
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                string DOCDT = data[0].retStr();
                //string TAXGRPCD = data[1].retStr();
                //string GOCD = data[2].retStr() == "" ? "" : data[2].retStr().retSqlformat();
                //string PRCCD = data[3].retStr();
                //string MTRLJOBCD = data[4].retStr();
                if (val.retStr() == "")
                {
                    var str = masterHelp.T_TXN_BARNO_help(val, VE.MENU_PARA, DOCDT, "", "", "", "");
                    if (str.IndexOf("='helpmnu'") >= 0)
                    {
                        return PartialView("_Help2", str);
                    }
                    else
                    {
                        return Content(str);
                    }
                }
                else
                {
                    var str = "";
                    DataTable stock_data = new DataTable();

                    stock_data = salesfunc.GetStock(DOCDT.retStr(), "", val.retStr().retSqlformat(), "", "", "", "", "", "", "");


                    if (stock_data == null || stock_data.Rows.Count == 0)//stock zero then return bardet from item master as blur
                    {
                        return Content("0");
                    }
                    else {
                        str = masterHelp.ToReturnFieldValues("", stock_data);
                        string all_gstper = ""; double gst = 0;
                        if (stock_data.Rows[0]["PRODGRPGSTPER"].retStr() != "")
                        {
                            all_gstper = salesfunc.retGstPer(stock_data.Rows[0]["PRODGRPGSTPER"].retStr(), stock_data.Rows[0]["RATE"].retDbl());
                            if (all_gstper.retStr() != "")
                            {
                                var gst_data = all_gstper.Split(',').ToList();
                                gst = (from a in gst_data select a.retDbl()).Sum();
                            }
                        }

                        str += "^ALL_GSTPER=^" + all_gstper + Cn.GCS();
                        str += "^GSTPER=^" + gst + Cn.GCS();
                        VE.TsalePos_TBATCHDTL = (from DataRow dr in stock_data.Rows
                                                 select new TsalePos_TBATCHDTL
                                                 {
                                                     BARNO = dr["BARNO"].retStr(),
                                                     ITGRPCD = dr["ITGRPCD"].retStr(),
                                                     ITGRPNM = dr["ITGRPNM"].retStr(),
                                                     MTRLJOBNM = dr["MTRLJOBNM"].retStr(),
                                                     MTBARCODE = dr["MTBARCODE"].retStr(),
                                                     MTRLJOBCD = dr["MTRLJOBCD"].retStr(),
                                                     ITSTYLE = dr["STYLENO"].retStr() + "" + dr["ITNM"].retStr(),
                                                     ITCD = dr["ITCD"].retStr(),
                                                     STYLENO = dr["STYLENO"].retStr(),
                                                     PARTNM = dr["PARTNM"].retStr(),
                                                     PRTBARCODE = dr["PRTBARCODE"].retStr(),
                                                     PARTCD = dr["PARTCD"].retStr(),
                                                     COLRCD = dr["COLRCD"].retStr(),
                                                     COLRNM = dr["COLRNM"].retStr(),
                                                     CLRBARCODE = dr["CLRBARCODE"].retStr(),
                                                     SIZENM = dr["SIZENM"].retStr(),
                                                     SZBARCODE = dr["SZBARCODE"].retStr(),
                                                     SIZECD = dr["SIZECD"].retStr(),
                                                     UOM = dr["uomcd"].retStr(),
                                                     STKTYPE = dr["STKTYPE"].retStr(),
                                                     FLAGMTR = dr["FLAGMTR"].retDbl(),
                                                     RATE = dr["RATE"].retDbl(),
                                                     HSNCODE = dr["HSNCODE"].retStr(),
                                                     PRODGRPGSTPER = dr["PRODGRPGSTPER"].retStr(),
                                                     ALL_GSTPER = all_gstper,
                                                     GSTPER = gst,
                                                     SHADE = dr["SHADE"].retStr(),
                                                     LOCABIN = dr["LOCABIN"].retStr(),
                                                     NEGSTOCK = dr["NEGSTOCK"].retStr(),
                                                     GLCD = VE.MENU_PARA == "SBCM" ? dr["SALGLCD"].retStr() : VE.MENU_PARA == "SBCMR" ? dr["SALGLCD"].retStr() : "",
                                                     BARGENTYPE = dr["BARGENTYPE"].retStr(),
                                                     BarImages = dr["BARIMAGE"].retStr(),

                                                 }).ToList();
                        int slno = 0, txnslno = 0;
                        for (int p = 0; p <= VE.TsalePos_TBATCHDTL.Count - 1; p++)
                        {
                            VE.TsalePos_TBATCHDTL[p].SLNO = Convert.ToByte(slno + 1);
                            VE.TsalePos_TBATCHDTL[p].TXNSLNO = Convert.ToByte(txnslno + 1);
                            VE.TsalePos_TBATCHDTL[p].DropDown_list1 = DropDown_list1();
                            VE.TsalePos_TBATCHDTL[p].DISC_TYPE = masterHelp.DISC_TYPE();
                            VE.TsalePos_TBATCHDTL[p].TDDISC_TYPE = DropDown_list2();
                            VE.TsalePos_TBATCHDTL[p].SCMDISC_TYPE = DropDown_list3();
                        }
                        VE.DefaultView = true;
                        return PartialView("_T_SALE_POS_PRODUCT", VE);
                    }


                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public List<DropDown_list2> DropDown_list2()
        {
            List<DropDown_list2> DTYP = new List<DropDown_list2>();
            DropDown_list2 DTYP3 = new DropDown_list2();
            DTYP3.text = "%";
            DTYP3.value = "P";
            DTYP.Add(DTYP3);
            DropDown_list2 DTYP2 = new DropDown_list2();
            DTYP2.text = "Nos";
            DTYP2.value = "N";
            DTYP.Add(DTYP2);
            DropDown_list2 DTYP1 = new DropDown_list2();
            DTYP1.text = "Qnty";
            DTYP1.value = "Q";
            DTYP.Add(DTYP1);
            DropDown_list2 DTYP4 = new DropDown_list2();
            DTYP4.text = "Fixed";
            DTYP4.value = "F";
            DTYP.Add(DTYP4);
            return DTYP;
        }
        public List<DropDown_list3> DropDown_list3()
        {
            List<DropDown_list3> DTYP = new List<DropDown_list3>();
            DropDown_list3 DTYP3 = new DropDown_list3();
            DTYP3.text = "%";
            DTYP3.value = "P";
            DTYP.Add(DTYP3);
            DropDown_list3 DTYP2 = new DropDown_list3();
            DTYP2.text = "Nos";
            DTYP2.value = "N";
            DTYP.Add(DTYP2);
            DropDown_list3 DTYP1 = new DropDown_list3();
            DTYP1.text = "Qnty";
            DTYP1.value = "Q";
            DTYP.Add(DTYP1);
            DropDown_list3 DTYP4 = new DropDown_list3();
            DTYP4.text = "Fixed";
            DTYP4.value = "F";
            DTYP.Add(DTYP4);
            return DTYP;
        }
        public ActionResult GetOrderDetails(TransactionSaleEntry VE, string val, string Code)
        {
            try
            {
                Cn.getQueryString(VE);
                var tbl = (List<PENDINGORDER>)TempData["PENDORDER" + VE.MENU_PARA]; TempData.Keep();
                if (tbl == null || tbl.Count == 0)
                {
                    return Content("Please Select Order!");
                }
                else
                {

                    foreach (var v in tbl)
                    {
                        double currentadjqnty = 0;
                        if (VE.TBATCHDTL != null)
                        {
                            currentadjqnty = VE.TBATCHDTL.Where(a => a.ORDAUTONO == v.ORDAUTONO && a.ORDSLNO.retStr() == v.ORDSLNO).Sum(b => b.QNTY).retDbl();
                        }
                        v.CURRENTADJQTY = currentadjqnty.retDbl();
                    }
                    tbl = tbl.Where(a => a.BALQTY - a.CURRENTADJQTY > 0).ToList();

                }
                if (Code.retStr() != "")
                {
                    tbl = tbl.Where(a => a.ITCD == Code).ToList();
                }
                if (val.retStr() != "")
                {
                    tbl = tbl.Where(a => a.ORDDOCNO == val).ToList();
                }

                if (val.retStr() == "" || tbl.Count > 1)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= tbl.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + tbl[i].ORDDOCNO + "</td><td>" + tbl[i].ORDDOCDT + " </td></tr>");
                    }
                    var hdr = "Order No." + Cn.GCS() + "Order Date";
                    return PartialView("_Help2", masterHelp.Generate_help(hdr, SB.ToString()));
                }
                else
                {
                    if (tbl.Count > 0)
                    {
                        string str = masterHelp.ToReturnFieldValues(tbl);
                        return Content(str);
                    }
                    else
                    {
                        return Content("Invalid Order No ! Please Enter a Valid Order No !!");
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult FillDetailData(TransactionSalePosEntry VE)
        {
            try
            {
                Cn.getQueryString(VE);
                VE.TTXNDTL = (from x in VE.TsalePos_TBATCHDTL
                              group x by new
                              {
                                  x.TXNSLNO,
                                  x.ITGRPCD,
                                  x.ITGRPNM,
                                  x.MTRLJOBCD,
                                  x.MTRLJOBNM,
                                  x.MTBARCODE,
                                  x.ITCD,
                                  x.ITSTYLE,
                                  x.STYLENO,
                                  x.DISCTYPE,
                                  x.DISCTYPE_DESC,
                                  x.TDDISCTYPE,
                                  x.TDDISCTYPE_DESC,
                                  x.SCMDISCTYPE,
                                  x.SCMDISCTYPE_DESC,
                                  x.UOM,
                                  x.STKTYPE,
                                  x.RATE,
                                  x.DISCRATE,
                                  x.SCMDISCRATE,
                                  x.TDDISCRATE,
                                  x.GSTPER,
                                  x.ALL_GSTPER,
                                  x.FLAGMTR,
                                  x.HSNCODE,
                                  x.PRODGRPGSTPER,
                                  x.BALENO,
                                  x.GLCD,
                                  x.ITREM,
                              } into P
                              select new TTXNDTL
                              {
                                  SLNO = P.Key.TXNSLNO.retShort(),
                                  ITGRPCD = P.Key.ITGRPCD,
                                  ITGRPNM = P.Key.ITGRPNM,
                                  MTRLJOBCD = P.Key.MTRLJOBCD,
                                  MTRLJOBNM = P.Key.MTRLJOBNM,
                                  // MTBARCODE = P.Key.MTBARCODE,
                                  ITCD = P.Key.ITCD,
                                  //  ITNM = P.Key.STYLENO + "" + P.Key.ITNM,
                                  ITSTYLE = P.Key.ITSTYLE,
                                  STYLENO = P.Key.STYLENO,
                                  STKTYPE = P.Key.STKTYPE,
                                  UOM = P.Key.UOM,
                                  NOS = P.Sum(A => A.NOS),
                                  QNTY = P.Sum(A => A.QNTY),
                                  FLAGMTR = P.Sum(A => A.FLAGMTR),
                                  BLQNTY = P.Sum(A => A.BLQNTY),
                                  RATE = P.Key.RATE,
                                  DISCTYPE = P.Key.DISCTYPE,
                                  DISCTYPE_DESC = P.Key.DISCTYPE_DESC,
                                  DISCRATE = P.Key.DISCRATE,
                                  TDDISCRATE = P.Key.TDDISCRATE,
                                  TDDISCTYPE = P.Key.TDDISCTYPE,
                                  TDDISCTYPE_DESC = P.Key.TDDISCTYPE_DESC,
                                  SCMDISCRATE = P.Key.SCMDISCRATE,
                                  SCMDISCTYPE = P.Key.SCMDISCTYPE,
                                  SCMDISCTYPE_DESC = P.Key.SCMDISCTYPE_DESC,
                                  ALL_GSTPER = P.Key.ALL_GSTPER.retStr() == "" ? P.Key.GSTPER.retStr() : P.Key.ALL_GSTPER,
                                  AMT = P.Sum(A => A.BLQNTY).retDbl() == 0 ? (P.Sum(A => A.QNTY).retDbl() - P.Sum(A => A.FLAGMTR).retDbl()) * P.Key.RATE.retDbl() : P.Sum(A => A.BLQNTY).retDbl() * P.Key.RATE.retDbl(),
                                  HSNCODE = P.Key.HSNCODE,
                                  PRODGRPGSTPER = P.Key.PRODGRPGSTPER,
                                  BALENO = P.Key.BALENO,
                                  GLCD = P.Key.GLCD,
                                  ITREM = P.Key.ITREM,
                              }).ToList();

                for (int p = 0; p <= VE.TTXNDTL.Count - 1; p++)
                {
                    if (VE.TTXNDTL[p].ALL_GSTPER.retStr() != "")
                    {
                        var tax_data = VE.TTXNDTL[p].ALL_GSTPER.Split(',').ToList();
                        if (tax_data.Count == 1)
                        {
                            VE.TTXNDTL[p].IGSTPER = tax_data[0].retDbl() / 3;
                            VE.TTXNDTL[p].CGSTPER = tax_data[0].retDbl() / 3;
                            VE.TTXNDTL[p].SGSTPER = tax_data[0].retDbl() / 3;
                        }
                        else
                        {
                            VE.TTXNDTL[p].IGSTPER = tax_data[0].retDbl();
                            VE.TTXNDTL[p].CGSTPER = tax_data[1].retDbl();
                            VE.TTXNDTL[p].SGSTPER = tax_data[2].retDbl();
                        }

                    }
                }
                VE.T_NOS = VE.TTXNDTL.Select(a => a.NOS).Sum().retDbl();
                VE.T_QNTY = VE.TTXNDTL.Select(a => a.QNTY).Sum().retDbl();
                VE.T_AMT = VE.TTXNDTL.Select(a => a.AMT).Sum().retDbl();
                VE.T_GROSS_AMT = VE.TTXNDTL.Select(a => a.TXBLVAL).Sum().retDbl();
                VE.T_IGST_AMT = VE.TTXNDTL.Select(a => a.IGSTAMT).Sum().retDbl();
                VE.T_CGST_AMT = VE.TTXNDTL.Select(a => a.CGSTAMT).Sum().retDbl();
                VE.T_SGST_AMT = VE.TTXNDTL.Select(a => a.SGSTAMT).Sum().retDbl();
                VE.T_CESS_AMT = VE.TTXNDTL.Select(a => a.CESSAMT).Sum().retDbl();
                VE.T_NET_AMT = VE.TTXNDTL.Select(a => a.NETAMT).Sum().retDbl();
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_SALE_POS_DETAIL", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetPackingSlip(string val, string Code)
        {
            try
            {
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                string autono = data[0];
                string docdt = data[1];
                string slcd = data[2];
                var str = masterHelp.PACKSLIPAUTONO_help(val, autono, docdt, slcd);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        private void FreightCharges(TransactionSalePosEntry VE, string AUTO_NO)
        {
            try
            {
                double A_T_CURR_AMT = 0; double A_T_AMT = 0; double A_T_TAXABLE = 0; double A_T_IGST_AMT = 0; double A_T_CGST_AMT = 0;
                double A_T_SGST_AMT = 0; double A_T_CESS_AMT = 0; double A_T_DUTY_AMT = 0; double A_T_NET_AMT = 0; double IGST_PER = 0; double CGST_PER = 0; double SGST_PER = 0;
                double CESS_PER = 0; double DUTY_PER = 0;
                string sql = "", scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
                Cn.getQueryString(VE);
                string S_P = "";
                S_P = "S"; 

                sql = "";
                sql += "select a.amtcd, b.amtnm, b.calccode, b.addless, b.taxcode, b.calctype, b.calcformula, ";
                sql += "a.amtdesc, b.glcd, b.hsncode, a.amtrate, a.curr_amt, a.amt,a.igstper, a.igstamt, ";
                sql += "a.sgstper, a.sgstamt, a.cgstper, a.cgstamt,a.cessper, a.cessamt, a.dutyper, a.dutyamt ";
                sql += "from " + scm + ".t_txnamt a, " + scm + ".m_amttype b ";
                sql += "where a.amtcd=b.amtcd(+) and b.salpur='" + S_P + "' and a.autono='" + AUTO_NO + "' ";
                sql += "union ";
                sql += "select b.amtcd, b.amtnm, b.calccode, b.addless, b.taxcode, b.calctype, b.calcformula, ";
                sql += "'' amtdesc, b.glcd, b.hsncode, 0 amtrate, 0 curr_amt, 0 amt,0 igstper, 0 igstamt, ";
                sql += "0 sgstper, 0 sgstamt, 0 cgstper, 0 cgstamt, 0 cessper, 0 cessamt, 0 dutyper, 0 dutyamt ";
                sql += "from " + scm + ".m_amttype b, " + scm + ".m_cntrl_hdr c ";
                sql += "where b.m_autono=c.m_autono(+) and b.salpur='" + S_P + "'  and nvl(c.inactive_tag,'N') = 'N' ";
                sql += "and b.amtcd not in (select amtcd from " + scm + ".t_txnamt where autono='" + AUTO_NO + "')";
                var AMOUNT_DATA = masterHelp.SQLquery(sql);


                VE.TTXNAMT = (from DataRow dr in AMOUNT_DATA.Rows
                              select new TTXNAMT()
                              {
                                  AMTCD = dr["amtcd"].ToString(),
                                  ADDLESS = dr["addless"].ToString(),
                                  TAXCODE = dr["taxcode"].ToString(),
                                  GLCD = dr["GLCD"].ToString(),
                                  AMTNM = dr["amtnm"].ToString(),
                                  CALCCODE = dr["calccode"].ToString(),
                                  CALCTYPE = dr["CALCTYPE"].ToString(),
                                  CALCFORMULA = dr["calcformula"].ToString(),
                                  AMTDESC = dr["amtdesc"].ToString(),
                                  HSNCODE = dr["hsncode"].ToString(),
                                  AMTRATE = dr["amtrate"].retDbl(),
                                  CURR_AMT = dr["curr_amt"].retDbl(),
                                  AMT = dr["amt"].retDbl(),
                                  IGSTPER = dr["igstper"].retDbl(),
                                  CGSTPER = dr["cgstper"].retDbl(),
                                  SGSTPER = dr["sgstper"].retDbl(),
                                  CESSPER = dr["cessper"].retDbl(),
                                  DUTYPER = dr["dutyper"].retDbl(),
                                  IGSTAMT = dr["igstamt"].retDbl(),
                                  CGSTAMT = dr["cgstamt"].retDbl(),
                                  SGSTAMT = dr["sgstamt"].retDbl(),
                                  CESSAMT = dr["cessamt"].retDbl(),
                                  DUTYAMT = dr["dutyamt"].retDbl(),
                              }).ToList();

                for (int p = 0; p <= VE.TTXNAMT.Count - 1; p++)
                {
                    VE.TTXNAMT[p].SLNO = Convert.ToInt16(p + 1);

                    VE.TTXNAMT[p].NETAMT = VE.TTXNAMT[p].AMT.Value + VE.TTXNAMT[p].IGSTAMT.Value + VE.TTXNAMT[p].CGSTAMT.Value + VE.TTXNAMT[p].SGSTAMT.Value + VE.TTXNAMT[p].CESSAMT.Value + VE.TTXNAMT[p].DUTYAMT.Value;
                    VE.TTXNAMT[p].IGSTPER = IGST_PER;
                    VE.TTXNAMT[p].CGSTPER = CGST_PER;
                    VE.TTXNAMT[p].SGSTPER = SGST_PER;
                    VE.TTXNAMT[p].CESSPER = CESS_PER;
                    A_T_CURR_AMT = A_T_CURR_AMT + VE.TTXNAMT[p].CURR_AMT.Value;
                    A_T_AMT = A_T_AMT + VE.TTXNAMT[p].AMT.Value;
                    A_T_IGST_AMT = A_T_IGST_AMT + VE.TTXNAMT[p].IGSTAMT.Value;
                    A_T_CGST_AMT = A_T_CGST_AMT + VE.TTXNAMT[p].CGSTAMT.Value;
                    A_T_SGST_AMT = A_T_SGST_AMT + VE.TTXNAMT[p].SGSTAMT.Value;
                    A_T_CESS_AMT = A_T_CESS_AMT + VE.TTXNAMT[p].CESSAMT.Value;
                    A_T_DUTY_AMT = A_T_DUTY_AMT + VE.TTXNAMT[p].DUTYAMT.Value;
                    A_T_NET_AMT = A_T_NET_AMT + A_T_AMT + A_T_IGST_AMT + A_T_CGST_AMT + A_T_SGST_AMT + A_T_CESS_AMT + A_T_DUTY_AMT;
                }
                VE.A_T_CURR = A_T_CURR_AMT;
                VE.A_T_AMOUNT = A_T_AMT;
                VE.A_T_IGST = A_T_IGST_AMT;
                VE.A_T_CGST = A_T_CGST_AMT;
                VE.A_T_SGST = A_T_SGST_AMT;
                VE.A_T_CESS = A_T_CESS_AMT;
                VE.A_T_DUTY = A_T_DUTY_AMT;
                VE.A_T_NET = A_T_NET_AMT;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
        }
        public ActionResult DeleteRowBarno(TransactionSalePosEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                List<TsalePos_TBATCHDTL> TsalePos_TBATCHDTL = new List<TsalePos_TBATCHDTL>();
                int count = 0;
                for (int i = 0; i <= VE.TsalePos_TBATCHDTL.Count - 1; i++)
                {
                    if (VE.TsalePos_TBATCHDTL[i].Checked == false)
                    {
                        count += 1;
                        TsalePos_TBATCHDTL item = new TsalePos_TBATCHDTL();
                        item = VE.TsalePos_TBATCHDTL[i];
                        if (VE.DefaultAction == "A")
                        {
                            item.SLNO = Convert.ToSByte(count);
                        }

                        TsalePos_TBATCHDTL.Add(item);
                    }
                }
                VE.TsalePos_TBATCHDTL = TsalePos_TBATCHDTL;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_T_SALE_POS_PRODUCT", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddDOCRow(TransactionSalePosEntry VE)
        {
            try
            {
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                var doctP = (from i in DB1.MS_DOCCTG
                             select new DocumentType()
                             {
                                 value = i.DOC_CTG,
                                 text = i.DOC_CTG
                             }).OrderBy(s => s.text).ToList();
                if (VE.UploadDOC == null)
                {
                    List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
                    UploadDOC MLI = new UploadDOC();
                    MLI.DocumentType = doctP;
                    MLocIFSC1.Add(MLI);
                    VE.UploadDOC = MLocIFSC1;
                }
                else
                {
                    List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
                    for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
                    {
                        UploadDOC MLI = new UploadDOC();
                        MLI = VE.UploadDOC[i];
                        MLI.DocumentType = doctP;
                        MLocIFSC1.Add(MLI);
                    }
                    UploadDOC MLI1 = new UploadDOC();
                    MLI1.DocumentType = doctP;
                    MLocIFSC1.Add(MLI1);
                    VE.UploadDOC = MLocIFSC1;
                }
                VE.DefaultView = true;
                return PartialView("_UPLOADDOCUMENTS", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult DeleteDOCRow(TransactionSalePosEntry VE)
        {
            try
            {
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                var doctP = (from i in DB1.MS_DOCCTG
                             select new DocumentType()
                             {
                                 value = i.DOC_CTG,
                                 text = i.DOC_CTG
                             }).OrderBy(s => s.text).ToList();
                List<UploadDOC> LOCAIFSC = new List<UploadDOC>();
                int count = 0;
                for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
                {
                    if (VE.UploadDOC[i].chk == false)
                    {
                        count += 1;
                        UploadDOC IFSC = new UploadDOC();
                        IFSC = VE.UploadDOC[i];
                        IFSC.DocumentType = doctP;
                        LOCAIFSC.Add(IFSC);
                    }
                }
                VE.UploadDOC = LOCAIFSC;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_UPLOADDOCUMENTS", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult cancelRecords(TransactionSalePosEntry VE, string par1)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                Cn.getQueryString(VE);
                using (var transaction = DB.Database.BeginTransaction())
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
                    T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                    if (par1 == "*#*")
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_TXN.AUTONO, CommVar.CurSchema(UNQSNO).ToString());
                    }
                    else
                    {
                        TCH = Cn.T_CONTROL_HDR(VE.T_TXN.AUTONO, CommVar.CurSchema(UNQSNO).ToString(), par1);
                    }
                    DB.Entry(TCH).State = System.Data.Entity.EntityState.Modified;
                    DB.SaveChanges();
                    transaction.Commit();
                }
                using (var transaction = DB.Database.BeginTransaction())
                {
                    DBF.Database.ExecuteSqlCommand("lock table " + CommVar.FinSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode");
                    T_CNTRL_HDR TCH1 = new T_CNTRL_HDR();
                    if (par1 == "*#*")
                    {
                        TCH1 = Cn.T_CONTROL_HDR(VE.T_TXN.AUTONO, CommVar.FinSchema(UNQSNO));
                    }
                    else
                    {
                        TCH1 = Cn.T_CONTROL_HDR(VE.T_TXN.AUTONO, CommVar.FinSchema(UNQSNO), par1);
                    }
                    DBF.Entry(TCH1).State = System.Data.Entity.EntityState.Modified;
                    DBF.SaveChanges();
                    transaction.Commit();
                }
                return Content("1");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult ParkRecord(FormCollection FC, TransactionSalePosEntry stream, string MNUDET, string UNQSNO)
        {
            try
            {
                if (stream.T_TXN.DOCCD.retStr() != "")
                {
                    stream.T_CNTRL_HDR.DOCCD = stream.T_TXN.DOCCD.retStr();
                }
                var menuID = MNUDET.Split('~')[0];
                var menuIndex = MNUDET.Split('~')[1];
                string ID = menuID + menuIndex + CommVar.Loccd(UNQSNO) + CommVar.Compcd(UNQSNO) + CommVar.CurSchema(UNQSNO) + "*" + DateTime.Now;
                ID = ID.Replace(" ", "_");
                string Userid = Session["UR_ID"].ToString();
                INI Handel_ini = new INI();
                var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                string JR = javaScriptSerializer.Serialize(stream);
                Handel_ini.IniWriteValue(Userid, ID, Cn.Encrypt(JR), Server.MapPath("~/Park.ini"));
                return Content("1");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public string RetrivePark(string value)
        {
            try
            {
                string url = masterHelp.RetriveParkFromFile(value, Server.MapPath("~/Park.ini"), Session["UR_ID"].ToString(), "Improvar.ViewModels.TransactionSalePosEntry");
                return url;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return ex.Message;
            }
        }
        public ActionResult SAVE(FormCollection FC, TransactionSalePosEntry VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            //  Oracle Queries
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            OracleTransaction OraTrans;
            string dbsql = "";
            string[] dbsql1;
            string dberrmsg = "";

            OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            OraCmd.Transaction = OraTrans;
            DB.Configuration.ValidateOnSaveEnabled = false;
            try
            {
                DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".T_CNTRL_HDR in  row share mode");
                OraCmd.CommandText = "lock table " + CommVar.CurSchema(UNQSNO) + ".T_CNTRL_HDR in  row share mode"; OraCmd.ExecuteNonQuery();

                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string ContentFlg = "";
                if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                {
                    //checking barcode &txndtl pge itcd wise qnty, nos should match

                    var barcodedata = (from x in VE.TsalePos_TBATCHDTL
                                       group x by new { x.ITCD } into P
                                       select new
                                       {
                                           ITCD = P.Key.ITCD,
                                           QTY = P.Sum(A => A.QNTY),
                                           NOS = P.Sum(A => A.NOS)
                                       }).Where(a => a.QTY != 0).ToList();
                    var txndtldata = (from x in VE.TTXNDTL
                                      group x by new { x.ITCD } into P
                                      select new
                                      {
                                          ITCD = P.Key.ITCD,
                                          QTY = P.Sum(A => A.QNTY),
                                          NOS = P.Sum(A => A.NOS)
                                      }).Where(a => a.QTY != 0).ToList();

                    //        var difList = barcodedata.Where(a => !txndtldata.Any(a1 => a1.ITCD == a.ITCD && a1.QTY == a.QTY && a1.NOS == a.NOS))
                    //.Union(txndtldata.Where(a => !barcodedata.Any(a1 => a1.ITCD == a.ITCD && a1.QTY == a.QTY && a1.NOS == a.NOS))).ToList();
                    var difList = barcodedata.Where(a => !txndtldata.Any(a1 => a1.ITCD == a.ITCD && a1.QTY == a.QTY && a1.NOS == a.NOS)).ToList();

                    if (difList != null && difList.Count > 0)
                    {
                        OraTrans.Rollback();
                        OraCon.Dispose();
                        return Content("Barcode grid itcd wise qnty, nos should match !!");
                    }

                    T_TXN TTXN = new T_TXN();
                    T_TXNMEMO TTXNMEMO = new T_TXNMEMO();
                    T_TXNPYMT_HDR TTXNPYMTHDR = new T_TXNPYMT_HDR();
                    T_TXNOTH TTXNOTH = new T_TXNOTH();
                    T_TXN_LINKNO TTXNLINKNO = new T_TXN_LINKNO();
                    T_CNTRL_HDR TCH = new T_CNTRL_HDR();
                    T_VCH_GST TVCHGST = new T_VCH_GST();
                    T_CNTRL_DOC_PASS TCDP = new T_CNTRL_DOC_PASS();
                    string DOCPATTERN = "";
                    string docpassrem = "";
                    bool blactpost = true, blgstpost = true;
                    TTXN.DOCDT = VE.T_TXN.DOCDT;
                    string Ddate = Convert.ToString(TTXN.DOCDT);
                    TTXN.CLCD = CommVar.ClientCode(UNQSNO);
                    string auto_no = ""; string Month = "";

                    #region Posting to finance Preparation
                    int COUNTER = 0;
                    string expglcd = "";
                    string stkdrcr = "C", strqty = " Q=" + VE.TOTQNTY.toRound(2).ToString();
                    string trcd = "TR";
                    string revcharge = "";
                    string dr = ""; string cr = ""; int isl = 0; string strrem = "";
                    double igst = 0; double cgst = 0; double sgst = 0; double cess = 0; double duty = 0; double dbqty = 0; double dbamt = 0; double dbcurramt = 0;
                    double dbDrAmt = 0, dbCrAmt = 0;
                    blactpost = true; blgstpost = true;
                    string parglcd = "saldebglcd", parclass1cd = "";
                    string strblno = "", strbldt = "", strduedt = "", strrefno = "", strvtype = "BL";
                    dr = "D"; cr = "C";
                    string sslcd = TTXN.SLCD;
                    if (VE.PSLCD.retStr() != "") sslcd = VE.PSLCD.ToString();

                    switch (VE.MENU_PARA)
                    {
                        case "SBCM":
                            stkdrcr = "C"; trcd = "SB"; strrem = "Cash Memo" + strqty; break;
                        case "SBCMR":
                            stkdrcr = "D"; dr = "C"; cr = "D"; trcd = "SC"; strrem = "Cash Memo Credit Note" + strqty; break;
                    }

                    string slcdlink = "", slcdpara = VE.MENU_PARA;
                    //if (VE.MENU_PARA == "PR") slcdpara = "PB";
                    string sql = "";
                    sql = "select class1cd, " + parglcd + " glcd from " + CommVar.CurSchema(UNQSNO) + ".m_syscnfg ";
                    DataTable tblsys = masterHelp.SQLquery(sql);
                    if (tblsys.Rows.Count == 0)
                    {
                        dberrmsg = "Debtor/Creditor code not setup"; goto dbnotsave;
                    }

                    sql = "select b.rogl, b.tcsgl, a.class1cd, null class2cd, nvl(c.crlimit,0) crlimit, nvl(c.crdays,0) crdays, ";
                    sql += "'" + VE.TsalePos_TBATCHDTL[0].GLCD.retStr() + "' prodglcd, ";
                    //if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "PR") sql += "b.igst_p igstcd, b.cgst_p cgstcd, b.sgst_p sgstcd, b.cess_p cesscd, b.duty_p dutycd, ";
                    sql += "b.igst_s igstcd, b.cgst_s cgstcd, b.sgst_s sgstcd, b.cess_s cesscd, b.duty_s dutycd, ";
                    sql += "a.saldebglcd parglcd, ";
                    sql += "igst_rvi, cgst_rvi, sgst_rvi, cess_rvi, igst_rvo, cgst_rvo, sgst_rvo, cess_rvo ";
                    sql += "from " + scm1 + ".m_syscnfg a, " + scmf + ".m_post b, " + scm1 + ".m_subleg_com c ";
                    sql += "where c.slcd in('" + VE.TTXNSLSMN[0].SLMSLCD.retStr() + "',null) and ";
                    sql += "c.compcd in ('" + COM + "',null) ";
                    DataTable tbl = masterHelp.SQLquery(sql);

                    parglcd = tbl.Rows[0]["parglcd"].retStr(); parclass1cd = tbl.Rows[0]["class1cd"].retStr();

                    //   Calculate Others Amount from amount tab to distrubute into txndtl table
                    double _amtdist = 0, _baldist = 0, _rpldist = 0, _mult = 1;
                    double _amtdistq = 0, _baldistq = 0, _rpldistq = 0;
                    double titamt = 0, titqty = 0;
                    int lastitemno = 0;
                    if (VE.TTXNAMT != null)
                    {
                        for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                        {
                            if (VE.TTXNAMT[i].SLNO != 0 && VE.TTXNAMT[i].AMTCD != null && VE.TTXNAMT[i].AMT != 0)
                            {
                                if (VE.TTXNAMT[i].ADDLESS == "L") _mult = -1; else _mult = 1;
                                if (VE.TTXNAMT[i].TAXCODE == "TC") _amtdistq = _amtdistq + (Convert.ToDouble(VE.TTXNAMT[i].AMT) * _mult);
                                else _amtdist = _amtdist + (Convert.ToDouble(VE.TTXNAMT[i].AMT) * _mult);
                            }
                        }
                    }
                    for (int i = 0; i <= VE.TsalePos_TBATCHDTL.Count - 1; i++)
                    {
                        if (VE.TsalePos_TBATCHDTL[i].SLNO != 0 && VE.TsalePos_TBATCHDTL[i].ITCD != null)
                        {
                            titamt = titamt + VE.TsalePos_TBATCHDTL[i].AMT.retDbl();
                            titqty = titqty + Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].QNTY);
                            lastitemno = i;
                        }
                    }

                    _baldist = _amtdist; _baldistq = _amtdistq;
                    #endregion
                    if (VE.DefaultAction == "A")
                    {
                        TTXN.EMD_NO = 0;
                        TTXN.DOCCD = VE.T_TXN.DOCCD;
                        TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
                        TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
                        DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TTXN.DOCNO), TTXN.DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                        auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TTXN.DOCNO, TTXN.DOCCD, Ddate);
                        TTXN.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                        Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                        //TempData["LASTGOCD" + VE.MENU_PARA] = VE.T_TXN.GOCD;
                        TCH = Cn.T_CONTROL_HDR(TTXN.DOCCD, TTXN.DOCDT, TTXN.DOCNO, TTXN.AUTONO, Month, DOCPATTERN, VE.DefaultAction, scm1, null, null, "".retDbl(), null);
                    }
                    else
                    {
                        TTXN.DOCCD = VE.T_TXN.DOCCD;
                        TTXN.DOCNO = VE.T_TXN.DOCNO;
                        TTXN.AUTONO = VE.T_TXN.AUTONO;
                        Month = VE.T_CNTRL_HDR.MNTHCD;
                        TTXN.EMD_NO = Convert.ToByte((VE.T_CNTRL_HDR.EMD_NO == null ? 0 : VE.T_CNTRL_HDR.EMD_NO) + 1);
                        DOCPATTERN = VE.T_CNTRL_HDR.DOCNO;
                        TTXN.DTAG = "E";
                    }
                    TTXN.DOCTAG = VE.MENU_PARA.retStr().Length > 2 ? VE.MENU_PARA.retStr().Remove(2) : VE.MENU_PARA.retStr();
                    TTXN.GOCD = "DH";
                    TTXN.DUEDAYS = VE.T_TXN.DUEDAYS;
                    TTXN.PARGLCD = parglcd; 
                    TTXN.CLASS1CD = parclass1cd; 
                    TTXN.MENU_PARA = VE.T_TXN.MENU_PARA;
                    if (VE.DefaultAction == "E")
                    {
                        dbsql = masterHelp.TblUpdt("t_batchdtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                        var comp = DB1.T_BATCHMST.Where(x => x.AUTONO == TTXN.AUTONO).OrderBy(s => s.AUTONO).ToList();
                        foreach (var v in comp)
                        {
                            if (!VE.TsalePos_TBATCHDTL.Where(s => s.BARNO == v.BARNO && s.SLNO == v.SLNO).Any())
                            {
                                dbsql = masterHelp.TblUpdt("t_batchmst", TTXN.AUTONO, "E", "S", "BARNO = '" + v.BARNO + "' and SLNO = '" + v.SLNO + "' ");
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                            }
                        }
                        dbsql = masterHelp.TblUpdt("t_batchmst", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                     
                        dbsql = masterHelp.TblUpdt("t_txndtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        //dbsql = masterHelp.TblUpdt("t_txntrans", TTXN.AUTONO, "E");
                        //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txnoth", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                       
                            dbsql = masterHelp.TblUpdt("t_txn_linkno", TTXN.AUTONO, "E");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                     
                        dbsql = masterHelp.TblUpdt("t_txnamt", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txnpymt", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_txnslsmn", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                        dbsql = masterHelp.TblUpdt("t_cntrl_doc_pass", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                        // finance
                        dbsql = masterHelp.finTblUpdt("t_vch_gst", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_bl", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_class", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_det", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                        dbsql = masterHelp.finTblUpdt("t_vch_hdr", TTXN.AUTONO, "E");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                    }
                    
                    // -------------------------Other Info--------------------------//
                    TTXNOTH.AUTONO = TTXN.AUTONO;
                    TTXNOTH.EMD_NO = TTXN.EMD_NO;
                    TTXNOTH.CLCD = TTXN.CLCD;
                    TTXNOTH.DTAG = TTXN.DTAG;
                    //----------------------------------------------------------//
                    
                        //TTXNLINKNO.EMD_NO = TTXN.EMD_NO;
                        //TTXNLINKNO.CLCD = TTXN.CLCD;
                        //TTXNLINKNO.DTAG = TTXN.DTAG;
                        //TTXNLINKNO.TTAG = TTXN.TTAG;
                        //TTXNLINKNO.AUTONO = TTXN.AUTONO;
                    //TTXNLINKNO.LINKAUTONO = VE.T_TXN_LINKNO.LINKAUTONO;
                    // -------------------------Ttxn Memo--------------------------//   
                    TTXNMEMO.EMD_NO = TTXN.EMD_NO;
                    TTXNMEMO.CLCD = TTXN.CLCD;
                    TTXNMEMO.DTAG = TTXN.DTAG;
                    TTXNMEMO.TTAG = TTXN.TTAG;
                    TTXNMEMO.AUTONO = TTXN.AUTONO;
                    TTXNMEMO.RTDEBCD = VE.T_TXNMEMO.RTDEBCD;
                    TTXNMEMO.NM = VE.T_TXNMEMO.NM;
                    TTXNMEMO.MOBILE = VE.T_TXNMEMO.MOBILE;
                    TTXNMEMO.CITY = "ABC";
                    TTXNMEMO.ADDR = "XYZ";
                    //----------------------------------------------------------//
                    // -------------------------T_TXNPYMT_HDR--------------------------//   
                    TTXNPYMTHDR.EMD_NO = TTXN.EMD_NO;
                    TTXNPYMTHDR.CLCD = TTXN.CLCD;
                    TTXNPYMTHDR.DTAG = TTXN.DTAG;
                    TTXNPYMTHDR.TTAG = TTXN.TTAG;
                    TTXNPYMTHDR.AUTONO = TTXN.AUTONO;
                    TTXNPYMTHDR.RTDEBCD = VE.T_TXNMEMO.RTDEBCD;
                    TTXNPYMTHDR.DRCR = stkdrcr;

                    //----------------------------------------------------------//

                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "S", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelp.RetModeltoSql(TTXN, VE.DefaultAction);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    //dbsql = masterHelp.RetModeltoSql(TXNTRANS);
                    //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.RetModeltoSql(TTXNOTH);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    //dbsql = masterHelp.RetModeltoSql(TTXNLINKNO);
                    //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.RetModeltoSql(TTXNMEMO);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.RetModeltoSql(TTXNPYMTHDR);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    // SAVE T_CNTRL_HDR_UNIQNO
                    //string docbarcode = ""; string UNIQNO = salesfunc.retVchrUniqId(TTXN.DOCCD, TTXN.AUTONO);
                    //sql = "select doccd,docbarcode from " + CommVar.CurSchema(UNQSNO) + ".m_doctype_bar where doccd='" + TTXN.DOCCD + "'";
                    //DataTable dt = masterHelp.SQLquery(sql);
                    //if (dt != null && dt.Rows.Count > 0) docbarcode = dt.Rows[0]["docbarcode"].retStr();
                    //if (VE.DefaultAction == "A")
                    //{
                    //    T_CNTRL_HDR_UNIQNO TCHUNIQNO = new T_CNTRL_HDR_UNIQNO();
                    //    TCHUNIQNO.CLCD = TTXN.CLCD;
                    //    TCHUNIQNO.AUTONO = TTXN.AUTONO;
                    //    TCHUNIQNO.UNIQNO = UNIQNO;
                    //    dbsql = masterHelp.RetModeltoSql(TCHUNIQNO);
                    //    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    //}
                    //string lbatchini = "";
                    //sql = "select lbatchini from " + CommVar.FinSchema(UNQSNO) + ".m_loca where loccd='" + CommVar.Loccd(UNQSNO) + "' and compcd='" + CommVar.Compcd(UNQSNO) + "'";
                    //dt = masterHelp.SQLquery(sql);
                    //if (dt != null && dt.Rows.Count > 0)
                    //{
                    //    lbatchini = dt.Rows[0]["lbatchini"].retStr();
                    //}
                    //  END T_CNTRL_HDR_UNIQNO

                    //  create header record in finschema
                    if (blactpost == true || blgstpost == true)
                    {
                        dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(TTXN.AUTONO, VE.DefaultAction, "F", Month, TTXN.DOCCD, DOCPATTERN, TTXN.DOCDT.retStr(), TTXN.EMD_NO.retShort(), TTXN.DOCNO, Convert.ToDouble(TTXN.DOCNO), null, null, null, TTXN.SLCD);
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        double currrt = 111;
                        //if (TTXN.CURRRT != null) currrt = Convert.ToDouble(TTXN.CURRRT);
                        dbsql = masterHelp.InsVch_Hdr(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, null, null, "Y", null, trcd, "", "", TTXN.CURR_CD, currrt, "", revcharge);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                    }


                    VE.BALEYR = Convert.ToDateTime(CommVar.FinStartDate(UNQSNO)).Year.retStr();
                    for (int i = 0; i <= VE.TsalePos_TBATCHDTL.Count - 1; i++)
                    {
                        if (VE.TsalePos_TBATCHDTL[i].SLNO != 0 && VE.TsalePos_TBATCHDTL[i].ITCD != null && VE.TsalePos_TBATCHDTL[i].MTRLJOBCD != null && VE.TsalePos_TBATCHDTL[i].STKTYPE != null)
                        {
                            if (i == lastitemno) { _rpldist = _baldist; _rpldistq = _baldistq; }
                            else
                            {
                                if (_amtdist + _amtdistq == 0) { _rpldist = 0; _rpldistq = 0; }
                                else
                                {
                                    _rpldist = ((_amtdist / titamt) * VE.TsalePos_TBATCHDTL[i].AMT).retDbl().toRound();
                                    _rpldistq = ((_amtdistq / titqty) * Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].QNTY)).toRound();
                                }
                            }
                            _baldist = _baldist - _rpldist; _baldistq = _baldistq - _rpldistq;

                            COUNTER = COUNTER + 1;
                            T_TXNDTL TTXNDTL = new T_TXNDTL();
                            TTXNDTL.CLCD = TTXN.CLCD;
                            TTXNDTL.EMD_NO = TTXN.EMD_NO;
                            TTXNDTL.DTAG = TTXN.DTAG;
                            TTXNDTL.AUTONO = TTXN.AUTONO;
                            TTXNDTL.SLNO = VE.TsalePos_TBATCHDTL[i].SLNO;
                            TTXNDTL.MTRLJOBCD = VE.TsalePos_TBATCHDTL[i].MTRLJOBCD;
                            TTXNDTL.ITCD = VE.TsalePos_TBATCHDTL[i].ITCD;
                            TTXNDTL.PARTCD = VE.TsalePos_TBATCHDTL[i].PARTCD;
                            TTXNDTL.COLRCD = VE.TsalePos_TBATCHDTL[i].COLRCD;
                            TTXNDTL.SIZECD = VE.TsalePos_TBATCHDTL[i].SIZECD;
                            TTXNDTL.STKDRCR = stkdrcr;
                            TTXNDTL.STKTYPE = VE.TsalePos_TBATCHDTL[i].STKTYPE;
                            TTXNDTL.HSNCODE = VE.TsalePos_TBATCHDTL[i].HSNCODE;
                            TTXNDTL.ITREM = VE.TsalePos_TBATCHDTL[i].ITREM;
                            //TTXNDTL.PCSREM = VE.TsalePos_TBATCHDTL[i].PCSREM;
                            //TTXNDTL.FREESTK = VE.TsalePos_TBATCHDTL[i].FREESTK;
                            TTXNDTL.BATCHNO = VE.TsalePos_TBATCHDTL[i].BATCHNO;
                            TTXNDTL.BALEYR = VE.BALEYR;// VE.TTXNDTL[i].BALEYR;
                            //TTXNDTL.BALENO = VE.TTXNDTL[i].BALENO;
                            TTXNDTL.GOCD = "DH";
                            //TTXNDTL.JOBCD = VE.TTXNDTL[i].JOBCD;
                            TTXNDTL.NOS = VE.TsalePos_TBATCHDTL[i].NOS == null ? 0 : VE.TsalePos_TBATCHDTL[i].NOS;
                            TTXNDTL.QNTY = VE.TsalePos_TBATCHDTL[i].QNTY;
                            TTXNDTL.BLQNTY = VE.TsalePos_TBATCHDTL[i].BLQNTY;
                            TTXNDTL.RATE = VE.TsalePos_TBATCHDTL[i].RATE;
                            TTXNDTL.AMT = VE.TsalePos_TBATCHDTL[i].AMT;
                            TTXNDTL.FLAGMTR = VE.TsalePos_TBATCHDTL[i].FLAGMTR;
                            //TTXNDTL.TOTDISCAMT = VE.TsalePos_TBATCHDTL[i].TOTDISCAMT;
                            //TTXNDTL.TXBLVAL = VE.TsalePos_TBATCHDTL[i].TXBLVAL;
                            //TTXNDTL.IGSTPER = VE.TsalePos_TBATCHDTL[i].IGSTPER;
                            //TTXNDTL.CGSTPER = VE.TsalePos_TBATCHDTL[i].CGSTPER;
                            //TTXNDTL.SGSTPER = VE.TsalePos_TBATCHDTL[i].SGSTPER;
                            //TTXNDTL.CESSPER = VE.TsalePos_TBATCHDTL[i].CESSPER;
                            //TTXNDTL.DUTYPER = VE.TsalePos_TBATCHDTL[i].DUTYPER;
                            //TTXNDTL.IGSTAMT = VE.TsalePos_TBATCHDTL[i].IGSTAMT;
                            //TTXNDTL.CGSTAMT = VE.TsalePos_TBATCHDTL[i].CGSTAMT;
                            //TTXNDTL.SGSTAMT = VE.TsalePos_TBATCHDTL[i].SGSTAMT;
                            //TTXNDTL.CESSAMT = VE.TsalePos_TBATCHDTL[i].CESSAMT;
                            //TTXNDTL.DUTYAMT = VE.TsalePos_TBATCHDTL[i].DUTYAMT;
                            //TTXNDTL.NETAMT = VE.TsalePos_TBATCHDTL[i].NETAMT;
                            TTXNDTL.OTHRAMT = _rpldist + _rpldistq;
                            //TTXNDTL.AGDOCNO = VE.TsalePos_TBATCHDTL[i].AGDOCNO;
                            //TTXNDTL.AGDOCDT = VE.TsalePos_TBATCHDTL[i].AGDOCDT;
                            //TTXNDTL.SHORTQNTY = VE.TsalePos_TBATCHDTL[i].SHORTQNTY;
                            TTXNDTL.DISCTYPE = VE.TsalePos_TBATCHDTL[i].DISCTYPE;
                            TTXNDTL.DISCRATE = VE.TsalePos_TBATCHDTL[i].DISCRATE;
                            //TTXNDTL.DISCAMT = VE.TsalePos_TBATCHDTL[i].DISCAMT;
                            TTXNDTL.SCMDISCTYPE = VE.TsalePos_TBATCHDTL[i].SCMDISCTYPE;
                            TTXNDTL.SCMDISCRATE = VE.TsalePos_TBATCHDTL[i].SCMDISCRATE;
                            //TTXNDTL.SCMDISCAMT = VE.TsalePos_TBATCHDTL[i].SCMDISCAMT;
                            TTXNDTL.TDDISCTYPE = VE.TsalePos_TBATCHDTL[i].TDDISCTYPE;
                            TTXNDTL.TDDISCRATE = VE.TsalePos_TBATCHDTL[i].TDDISCRATE;
                            //TTXNDTL.TDDISCAMT = VE.TsalePos_TBATCHDTL[i].TDDISCAMT;
                            //TTXNDTL.PRCCD = VE.T_TXNOTH.PRCCD;
                            //TTXNDTL.PRCEFFDT = VE.TsalePos_TBATCHDTL[i].PRCEFFDT;
                            TTXNDTL.GLCD = VE.TsalePos_TBATCHDTL[i].GLCD;
                            dbsql = masterHelp.RetModeltoSql(TTXNDTL);
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                            dbqty = dbqty + Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].QNTY);
                            //igst = igst + Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].IGSTAMT);
                            //cgst = cgst + Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].CGSTAMT);
                            //sgst = sgst + Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].SGSTAMT);
                            //cess = cess + Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].CESSAMT);
                            //duty = duty + Convert.ToDouble(VE.TsalePos_TBATCHDTL[i].DUTYAMT);
                        }
                    }

                    COUNTER = 0; int COUNTERBATCH = 0; bool recoexist = false;
                    if (VE.TsalePos_TBATCHDTL != null && VE.TsalePos_TBATCHDTL.Count > 0)
                    {
                        for (int i = 0; i <= VE.TsalePos_TBATCHDTL.Count - 1; i++)
                        {
                            if (VE.TsalePos_TBATCHDTL[i].ITCD != null && VE.TsalePos_TBATCHDTL[i].QNTY != 0)
                            {
                                bool flagbatch = false;
                                string barno = "";
                             
                                    barno = salesfunc.GenerateBARNO(VE.TsalePos_TBATCHDTL[i].ITCD, VE.TsalePos_TBATCHDTL[i].MTBARCODE, VE.TsalePos_TBATCHDTL[i].PRTBARCODE, VE.TsalePos_TBATCHDTL[i].CLRBARCODE, VE.TsalePos_TBATCHDTL[i].SZBARCODE);
                                    sql = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where barno='" + barno + "'";
                                    OraCmd.CommandText = sql; var OraReco = OraCmd.ExecuteReader();
                                    if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                    if (recoexist == false) flagbatch = true;
                             

                               // checking barno exist or not
                                string Action = "", SqlCondition = "";
                                if (VE.DefaultAction == "A")
                                {
                                    Action = VE.DefaultAction;
                                }
                                else
                                {
                                    sql = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where autono='" + TTXN.AUTONO + "' and slno = " + VE.TsalePos_TBATCHDTL[i].SLNO + " and barno='" + barno + "' ";
                                    OraCmd.CommandText = sql;  OraReco = OraCmd.ExecuteReader();
                                    if (OraReco.HasRows == false) recoexist = false; else recoexist = true; OraReco.Dispose();

                                    if (recoexist == true)
                                    {
                                        Action = "E";
                                        SqlCondition = "autono = '" + TTXN.AUTONO + "' and slno = " + VE.TsalePos_TBATCHDTL[i].SLNO + " and barno='" + barno + "' ";
                                        flagbatch = true;
                                        barno = VE.TsalePos_TBATCHDTL[i].BARNO;
                                    }
                                    else
                                    {
                                        Action = "A";
                                    }

                                }

                                if (flagbatch == true)
                                {
                                    T_BATCHMST TBATCHMST = new T_BATCHMST();
                                    TBATCHMST.EMD_NO = TTXN.EMD_NO;
                                    TBATCHMST.CLCD = TTXN.CLCD;
                                    TBATCHMST.DTAG = TTXN.DTAG;
                                    TBATCHMST.TTAG = TTXN.TTAG;
                                    TBATCHMST.SLNO = VE.TsalePos_TBATCHDTL[i].SLNO; // ++COUNTERBATCH;
                                    TBATCHMST.AUTONO = TTXN.AUTONO;
                                    TBATCHMST.SLCD = TTXN.SLCD;
                                    TBATCHMST.MTRLJOBCD = VE.TsalePos_TBATCHDTL[i].MTRLJOBCD;
                                    TBATCHMST.STKTYPE = VE.TsalePos_TBATCHDTL[i].STKTYPE;
                                    TBATCHMST.JOBCD = TTXN.JOBCD;
                                    TBATCHMST.BARNO = barno;
                                    TBATCHMST.ITCD = VE.TsalePos_TBATCHDTL[i].ITCD;
                                    TBATCHMST.PARTCD = VE.TsalePos_TBATCHDTL[i].PARTCD;
                                    TBATCHMST.SIZECD = VE.TsalePos_TBATCHDTL[i].SIZECD;
                                    TBATCHMST.COLRCD = VE.TsalePos_TBATCHDTL[i].COLRCD;
                                    TBATCHMST.NOS = VE.TsalePos_TBATCHDTL[i].NOS;
                                    TBATCHMST.QNTY = VE.TsalePos_TBATCHDTL[i].QNTY;
                                    TBATCHMST.RATE = VE.TsalePos_TBATCHDTL[i].RATE;
                                    TBATCHMST.AMT = VE.TsalePos_TBATCHDTL[i].AMT;
                                    TBATCHMST.FLAGMTR = VE.TsalePos_TBATCHDTL[i].FLAGMTR;
                                    //TBATCHMST.MTRL_COST = VE.TsalePos_TBATCHDTL[i].MTRL_COST;
                                    //TBATCHMST.OTH_COST = VE.TsalePos_TBATCHDTL[i].OTH_COST;
                                    TBATCHMST.ITREM = VE.TsalePos_TBATCHDTL[i].ITREM;
                                    TBATCHMST.PDESIGN = VE.TsalePos_TBATCHDTL[i].PDESIGN;
                                    if (VE.T_TXN.BARGENTYPE == "E")
                                    {
                                        TBATCHMST.HSNCODE = VE.TsalePos_TBATCHDTL[i].HSNCODE;
                                      
                                            TBATCHMST.OURDESIGN = VE.TsalePos_TBATCHDTL[i].OURDESIGN;
                                            //if (VE.TsalePos_TBATCHDTL[i].BarImages.retStr() != "")
                                            //{
                                            //    var barimg = SaveBarImage(VE.TsalePos_TBATCHDTL[i].BarImages, barno, TTXN.EMD_NO.retShort());
                                            //    DB.T_BATCH_IMG_HDR.AddRange(barimg.Item1);
                                            //    DB.SaveChanges();
                                            //    var disntImgHdr = barimg.Item1.GroupBy(u => u.BARNO).Select(r => r.First()).ToList();
                                            //    foreach (var imgbar in disntImgHdr)
                                            //    {
                                            //        T_BATCH_IMG_HDR_LINK m_batchImglink = new T_BATCH_IMG_HDR_LINK();
                                            //        m_batchImglink.CLCD = TTXN.CLCD;
                                            //        m_batchImglink.EMD_NO = TTXN.EMD_NO;
                                            //        m_batchImglink.BARNO = imgbar.BARNO;
                                            //        m_batchImglink.MAINBARNO = imgbar.BARNO;
                                            //        DB.T_BATCH_IMG_HDR_LINK.Add(m_batchImglink);
                                            //    }
                                            //}
                                        
                                    }
                                    //TBATCHMST.ORGBATCHAUTONO = VE.TsalePos_TBATCHDTL[i].ORGBATCHAUTONO;
                                    //TBATCHMST.ORGBATCHSLNO = VE.TsalePos_TBATCHDTL[i].ORGBATCHSLNO;
                                    TBATCHMST.DIA = VE.TsalePos_TBATCHDTL[i].DIA;
                                    TBATCHMST.CUTLENGTH = VE.TsalePos_TBATCHDTL[i].CUTLENGTH;
                                    TBATCHMST.LOCABIN = VE.TsalePos_TBATCHDTL[i].LOCABIN;
                                    TBATCHMST.SHADE = VE.TsalePos_TBATCHDTL[i].SHADE;
                                    TBATCHMST.MILLNM = VE.TsalePos_TBATCHDTL[i].MILLNM;
                                    TBATCHMST.BATCHNO = VE.TsalePos_TBATCHDTL[i].BATCHNO;
                                    TBATCHMST.ORDAUTONO = VE.TsalePos_TBATCHDTL[i].ORDAUTONO;
                                    dbsql = masterHelp.RetModeltoSql(TBATCHMST);
                                    dbsql = masterHelp.RetModeltoSql(TBATCHMST, Action, "", SqlCondition);
                                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                                }
                                COUNTER = COUNTER + 1;
                                T_BATCHDTL TsalePos_TBATCHDTL = new T_BATCHDTL();
                                TsalePos_TBATCHDTL.EMD_NO = TTXN.EMD_NO;
                                TsalePos_TBATCHDTL.CLCD = TTXN.CLCD;
                                TsalePos_TBATCHDTL.DTAG = TTXN.DTAG;
                                TsalePos_TBATCHDTL.TTAG = TTXN.TTAG;
                                TsalePos_TBATCHDTL.AUTONO = TTXN.AUTONO;
                                TsalePos_TBATCHDTL.TXNSLNO = VE.TsalePos_TBATCHDTL[i].TXNSLNO;
                                TsalePos_TBATCHDTL.SLNO = VE.TsalePos_TBATCHDTL[i].SLNO;  //COUNTER.retShort();
                                TsalePos_TBATCHDTL.GOCD = VE.T_TXN.GOCD;
                                TsalePos_TBATCHDTL.BARNO = barno;
                                TsalePos_TBATCHDTL.MTRLJOBCD = VE.TsalePos_TBATCHDTL[i].MTRLJOBCD;
                                TsalePos_TBATCHDTL.PARTCD = VE.TsalePos_TBATCHDTL[i].PARTCD;
                                TsalePos_TBATCHDTL.HSNCODE = VE.TsalePos_TBATCHDTL[i].HSNCODE;
                                TsalePos_TBATCHDTL.STKDRCR = stkdrcr;
                                TsalePos_TBATCHDTL.NOS = VE.TsalePos_TBATCHDTL[i].NOS;
                                TsalePos_TBATCHDTL.QNTY = VE.TsalePos_TBATCHDTL[i].QNTY;
                                TsalePos_TBATCHDTL.BLQNTY = VE.TsalePos_TBATCHDTL[i].BLQNTY;
                                TsalePos_TBATCHDTL.FLAGMTR = VE.TsalePos_TBATCHDTL[i].FLAGMTR;
                                TsalePos_TBATCHDTL.ITREM = VE.TsalePos_TBATCHDTL[i].ITREM;
                                TsalePos_TBATCHDTL.RATE = VE.TsalePos_TBATCHDTL[i].RATE;
                                TsalePos_TBATCHDTL.DISCRATE = VE.TsalePos_TBATCHDTL[i].DISCRATE;
                                TsalePos_TBATCHDTL.DISCTYPE = VE.TsalePos_TBATCHDTL[i].DISCTYPE;
                                TsalePos_TBATCHDTL.SCMDISCRATE = VE.TsalePos_TBATCHDTL[i].SCMDISCRATE;
                                TsalePos_TBATCHDTL.SCMDISCTYPE = VE.TsalePos_TBATCHDTL[i].SCMDISCTYPE;
                                TsalePos_TBATCHDTL.TDDISCRATE = VE.TsalePos_TBATCHDTL[i].TDDISCRATE;
                                TsalePos_TBATCHDTL.TDDISCTYPE = VE.TsalePos_TBATCHDTL[i].TDDISCTYPE;
                                TsalePos_TBATCHDTL.ORDAUTONO = VE.TsalePos_TBATCHDTL[i].ORDAUTONO;
                                TsalePos_TBATCHDTL.ORDSLNO = VE.TsalePos_TBATCHDTL[i].ORDSLNO;
                                TsalePos_TBATCHDTL.DIA = VE.TsalePos_TBATCHDTL[i].DIA;
                                TsalePos_TBATCHDTL.CUTLENGTH = VE.TsalePos_TBATCHDTL[i].CUTLENGTH;
                                TsalePos_TBATCHDTL.LOCABIN = VE.TsalePos_TBATCHDTL[i].LOCABIN;
                                TsalePos_TBATCHDTL.SHADE = VE.TsalePos_TBATCHDTL[i].SHADE;
                                TsalePos_TBATCHDTL.MILLNM = VE.TsalePos_TBATCHDTL[i].MILLNM;
                                TsalePos_TBATCHDTL.BATCHNO = VE.TsalePos_TBATCHDTL[i].BATCHNO;
                                TsalePos_TBATCHDTL.BALEYR = VE.BALEYR;// VE.TsalePos_TBATCHDTL[i].BALEYR;
                                TsalePos_TBATCHDTL.BALENO = VE.TsalePos_TBATCHDTL[i].BALENO;
                                dbsql = masterHelp.RetModeltoSql(TsalePos_TBATCHDTL);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                              
                            }
                        }
                    }

                    if (dbqty == 0)
                    {
                        dberrmsg = "Quantity not entered"; goto dbnotsave;
                    }
                    isl = 1;

                    if (VE.TTXNAMT != null)
                    {
                        for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                        {
                            if (VE.TTXNAMT[i].SLNO != 0 && VE.TTXNAMT[i].AMTCD != null && VE.TTXNAMT[i].AMT != 0)
                            {
                                T_TXNAMT TTXNAMT = new T_TXNAMT();
                                TTXNAMT.AUTONO = TTXN.AUTONO;
                                TTXNAMT.SLNO = VE.TTXNAMT[i].SLNO;
                                TTXNAMT.EMD_NO = TTXN.EMD_NO;
                                TTXNAMT.CLCD = TTXN.CLCD;
                                TTXNAMT.DTAG = TTXN.DTAG;
                                TTXNAMT.AMTCD = VE.TTXNAMT[i].AMTCD;
                                TTXNAMT.AMTDESC = VE.TTXNAMT[i].AMTDESC;
                                TTXNAMT.AMTRATE = VE.TTXNAMT[i].AMTRATE;
                                TTXNAMT.HSNCODE = VE.TTXNAMT[i].HSNCODE;
                                TTXNAMT.CURR_AMT = VE.TTXNAMT[i].CURR_AMT;
                                TTXNAMT.AMT = VE.TTXNAMT[i].AMT;
                                TTXNAMT.IGSTPER = VE.TTXNAMT[i].IGSTPER;
                                TTXNAMT.IGSTAMT = VE.TTXNAMT[i].IGSTAMT;
                                TTXNAMT.CGSTPER = VE.TTXNAMT[i].CGSTPER;
                                TTXNAMT.CGSTAMT = VE.TTXNAMT[i].CGSTAMT;
                                TTXNAMT.SGSTPER = VE.TTXNAMT[i].SGSTPER;
                                TTXNAMT.SGSTAMT = VE.TTXNAMT[i].SGSTAMT;
                                TTXNAMT.CESSPER = VE.TTXNAMT[i].CESSPER;
                                TTXNAMT.CESSAMT = VE.TTXNAMT[i].CESSAMT;
                                TTXNAMT.DUTYPER = VE.TTXNAMT[i].DUTYPER;
                                TTXNAMT.DUTYAMT = VE.TTXNAMT[i].DUTYAMT;
                                dbsql = masterHelp.RetModeltoSql(TTXNAMT);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                                if (blactpost == true)
                                {
                                    isl = isl + 1;
                                    string adrcr = cr;
                                    if (VE.TTXNAMT[i].ADDLESS == "L") adrcr = dr;
                                    dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, VE.TTXNAMT[i].GLCD, sslcd,
                                            Convert.ToDouble(VE.TTXNAMT[i].AMT), strrem, parglcd, TTXN.SLCD, 0, 0, Convert.ToDouble(VE.TTXNAMT[i].CURR_AMT));
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    if (adrcr == "D") dbDrAmt = dbDrAmt + Convert.ToDouble(VE.TTXNAMT[i].AMT);
                                    else dbCrAmt = dbCrAmt + Convert.ToDouble(VE.TTXNAMT[i].AMT);

                                    //if (VE.TsalePos_TBATCHDTL != null)
                                    //{
                                    //    dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                    //            VE.TsalePos_TBATCHDTL[0].CLASS1CD, "", Convert.ToDouble(VE.TTXNAMT[i].AMT), 0, strrem);
                                    //    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    //}
                                }
                                igst = igst + Convert.ToDouble(VE.TTXNAMT[i].IGSTAMT);
                                cgst = cgst + Convert.ToDouble(VE.TTXNAMT[i].CGSTAMT);
                                sgst = sgst + Convert.ToDouble(VE.TTXNAMT[i].SGSTAMT);
                                cess = cess + Convert.ToDouble(VE.TTXNAMT[i].CESSAMT);
                                duty = duty + Convert.ToDouble(VE.TTXNAMT[i].DUTYAMT);
                            }
                        }
                    }
                    if (VE.TTXNPYMT != null)
                    {
                        for (int i = 0; i <= VE.TTXNPYMT.Count - 1; i++)
                        {
                            if (VE.TTXNPYMT[i].SLNO != 0 && VE.TTXNPYMT[i].AMT != 0)
                            {
                                T_TXNPYMT TTXNPYMNT = new T_TXNPYMT();
                                TTXNPYMNT.AUTONO = TTXN.AUTONO;
                                TTXNPYMNT.SLNO = VE.TTXNAMT[i].SLNO;
                                TTXNPYMNT.EMD_NO = TTXN.EMD_NO;
                                TTXNPYMNT.CLCD = TTXN.CLCD;
                                TTXNPYMNT.DTAG = TTXN.DTAG;
                                TTXNPYMNT.PYMTCD = VE.TTXNPYMT[i].PYMTCD;
                                TTXNPYMNT.AMT = VE.TTXNPYMT[i].AMT.retDbl();
                                TTXNPYMNT.CARDNO = VE.TTXNPYMT[i].CARDNO;
                                TTXNPYMNT.INSTNO = VE.TTXNPYMT[i].INSTNO;
                                TTXNPYMNT.INSTDT = Convert.ToDateTime(VE.TTXNPYMT[i].INSTDT);
                                TTXNPYMNT.PYMTREM = VE.TTXNPYMT[i].PYMTREM;
                                dbsql = masterHelp.RetModeltoSql(TTXNPYMNT);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    if (VE.TTXNSLSMN != null)
                    {
                        for (int i = 0; i <= VE.TTXNSLSMN.Count - 1; i++)
                        {
                            if (VE.TTXNSLSMN[i].SLNO != 0 && VE.TTXNSLSMN[i].SLMSLCD != null && VE.TTXNSLSMN[i].ITAMT != 0)
                            {
                                T_TXNSLSMN TTXNSLSMN = new T_TXNSLSMN();
                                TTXNSLSMN.AUTONO = TTXN.AUTONO;
                                TTXNSLSMN.SLMSLCD = VE.TTXNSLSMN[i].SLMSLCD;
                                TTXNSLSMN.EMD_NO = TTXN.EMD_NO;
                                TTXNSLSMN.CLCD = TTXN.CLCD;
                                TTXNSLSMN.DTAG = TTXN.DTAG;
                                TTXNSLSMN.PER = VE.TTXNSLSMN[i].PER.retDbl();
                                TTXNSLSMN.ITAMT = VE.TTXNSLSMN[i].ITAMT.retDbl();
                                TTXNSLSMN.BLAMT = VE.TTXNSLSMN[i].BLAMT.retDbl();
                                dbsql = masterHelp.RetModeltoSql(TTXNSLSMN);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    #region Document Passing checking
                    // -----------------------DOCUMENT PASSING DATA---------------------------//
                    double TRAN_AMT = Convert.ToDouble(TTXN.BLAMT);
                    if (docpassrem != "")
                    {
                        docpassrem = "Doc # " + TTXN.DOCNO.ToString() + " Party Name # " + VE.SLNM + " [" + VE.SLAREA + "]".ToString() + "~" + docpassrem;
                        var TCDP_DATA1 = Cn.T_CONTROL_DOC_PASS(TTXN.DOCCD, 0, TTXN.EMD_NO.Value, TTXN.AUTONO, CommVar.CurSchema(UNQSNO), docpassrem);
                        if (TCDP_DATA1.Item1.Count != 0)
                        {
                            DB.T_CNTRL_DOC_PASS.AddRange(TCDP_DATA1.Item1);
                        }
                    }
                    else
                    {
                        var TCDP_DATA = Cn.T_CONTROL_DOC_PASS(TTXN.DOCCD, TRAN_AMT, TTXN.EMD_NO.Value, TTXN.AUTONO, CommVar.CurSchema(UNQSNO));
                        if (TCDP_DATA.Item1.Count != 0)
                        {
                            DB.T_CNTRL_DOC_PASS.AddRange(TCDP_DATA.Item1);
                        }
                    }
                    if (docpassrem != "") DB.T_CNTRL_DOC_PASS.Add(TCDP);
                    #endregion

                    if (VE.UploadDOC != null)// add
                    {
                        var img = Cn.SaveUploadImageTransaction(VE.UploadDOC, TTXN.AUTONO, TTXN.EMD_NO.Value);
                        if (img.Item1.Count != 0)
                        {
                            for (int tr = 0; tr <= img.Item1.Count - 1; tr++)
                            {
                                dbsql = masterHelp.RetModeltoSql(img.Item1[tr]);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                            for (int tr = 0; tr <= img.Item2.Count - 1; tr++)
                            {
                                dbsql = masterHelp.RetModeltoSql(img.Item2[tr]);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    if (VE.T_CNTRL_HDR_REM.DOCREM != null)// add REMARKS
                    {
                        var NOTE = Cn.SAVETRANSACTIONREMARKS(VE.T_CNTRL_HDR_REM, TTXN.AUTONO, TTXN.CLCD, TTXN.EMD_NO.Value);
                        if (NOTE.Item1.Count != 0)
                        {
                            for (int tr = 0; tr <= NOTE.Item1.Count - 1; tr++)
                            {
                                dbsql = masterHelp.RetModeltoSql(NOTE.Item1[tr]);
                                dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                            }
                        }
                    }
                    #region Financial Posting
                    string salpur = "";
                    double itamt = 0;
                    if (blactpost == true)
                    {
                        salpur = "S";
                        string prodrem = strrem; expglcd = "";

                        var AMTGLCD = (from x in VE.TTXNDTL
                                       group x by new { x.GLCD, x.CLASS1CD } into P
                                       select new
                                       {
                                           GLCD = P.Key.GLCD,
                                           CLASS1CD = P.Key.CLASS1CD,
                                           QTY = P.Sum(A => A.QNTY),
                                           TXBLVAL = P.Sum(A => A.TXBLVAL)
                                       }).Where(a => a.QTY != 0).ToList();

                        if (AMTGLCD != null && AMTGLCD.Count > 0)
                        {
                            for (int i = 0; i <= AMTGLCD.Count - 1; i++)
                            {
                                isl = isl + 1;
                                dbamt = AMTGLCD[i].TXBLVAL.retDbl();
                                dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), cr, AMTGLCD[i].GLCD, sslcd,
                                        dbamt, prodrem, parglcd, TTXN.SLCD, AMTGLCD[i].QTY.retDbl(), 0, 0);
                                OraCmd.CommandText = dbsql;
                                OraCmd.ExecuteNonQuery();
                                if (cr == "D") dbDrAmt = dbDrAmt + dbamt;
                                else dbCrAmt = dbCrAmt + dbamt;
                                dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                        AMTGLCD[i].CLASS1CD, "", AMTGLCD[i].TXBLVAL.retDbl(), 0, strrem);
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                itamt = itamt + AMTGLCD[i].TXBLVAL.retDbl();
                                expglcd = AMTGLCD[i].GLCD;
                            }
                        }
                        #region  GST Financial Part
                        string[] gstpostcd = new string[5];
                        double[] gstpostamt = new double[5];
                        for (int gt = 0; gt < 5; gt++)
                        {
                            gstpostcd[gt] = ""; gstpostamt[gt] = 0;
                        }

                        int gi = 0;
                        if (revcharge != "Y")
                        {
                            gstpostcd[gi] = tbl.Rows[0]["igstcd"].ToString(); gstpostamt[gi] = igst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["cgstcd"].ToString(); gstpostamt[gi] = cgst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["sgstcd"].ToString(); gstpostamt[gi] = sgst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["cesscd"].ToString(); gstpostamt[gi] = cess; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["dutycd"].ToString(); gstpostamt[gi] = duty; gi++;
                        }
                        else
                        {
                            gstpostcd[gi] = tbl.Rows[0]["igst_rvi"].ToString(); gstpostamt[gi] = igst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["cgst_rvi"].ToString(); gstpostamt[gi] = cgst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["sgst_rvi"].ToString(); gstpostamt[gi] = sgst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["cess_rvi"].ToString(); gstpostamt[gi] = cess; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["dutycd"].ToString(); gstpostamt[gi] = duty; gi++;
                        }

                        for (int gt = 0; gt < 5; gt++)
                        {
                            if (gstpostamt[gt] != 0)
                            {
                                isl = isl + 1;
                                dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), cr, gstpostcd[gt], sslcd,
                                        gstpostamt[gt], prodrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, dbqty, 0, 0);
                                OraCmd.CommandText = dbsql;
                                OraCmd.ExecuteNonQuery();
                                if (cr == "D") dbDrAmt = dbDrAmt + gstpostamt[gt];
                                else dbCrAmt = dbCrAmt + gstpostamt[gt];
                                dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                        tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), gstpostamt[gt], 0, strrem);
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            }
                        }
                        if (revcharge == "Y")
                        {
                            gi = 0;
                            gstpostcd[gi] = tbl.Rows[0]["igst_rvo"].ToString(); gstpostamt[gi] = igst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["cgst_rvo"].ToString(); gstpostamt[gi] = cgst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["sgst_rvo"].ToString(); gstpostamt[gi] = sgst; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["cess_rvo"].ToString(); gstpostamt[gi] = cess; gi++;
                            gstpostcd[gi] = tbl.Rows[0]["dutycd"].ToString(); gstpostamt[gi] = duty; gi++;
                            for (int gt = 0; gt < 5; gt++)
                            {
                                if (gstpostamt[gt] != 0)
                                {
                                    isl = isl + 1;
                                    dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), dr, gstpostcd[gt], null,
                                            gstpostamt[gt], prodrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, dbqty, 0, 0);
                                    OraCmd.CommandText = dbsql;
                                    OraCmd.ExecuteNonQuery();
                                    if (dr == "D") dbDrAmt = dbDrAmt + gstpostamt[gt];
                                    else dbCrAmt = dbCrAmt + gstpostamt[gt];
                                    dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), null,
                                            tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), gstpostamt[gt], 0, strrem);
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                }
                            }
                        }
                        #endregion
                        //  TCS
                        dbamt = Convert.ToDouble(VE.T_TXN.TCSAMT);
                        if (dbamt != 0)
                        {
                            string adrcr = cr;

                            isl = isl + 1;
                            dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, tbl.Rows[0]["tcsgl"].ToString(), sslcd,
                                    dbamt, strrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, 0, 0, 0);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            if (adrcr == "D") dbDrAmt = dbDrAmt + dbamt;
                            else dbCrAmt = dbCrAmt + dbamt;
                            dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                    null, null, Convert.ToDouble(VE.T_TXN.TCSAMT), 0, strrem + " TCS @ " + VE.T_TXN.TCSPER.ToString() + "%");
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        }
                        //  Ronded off
                        dbamt = Convert.ToDouble(VE.T_TXN.ROAMT);
                        if (dbamt != 0)
                        {
                            string adrcr = cr;
                            if (dbamt < 0) adrcr = dr;
                            if (dbamt < 0) dbamt = dbamt * -1;

                            isl = isl + 1;
                            dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), adrcr, tbl.Rows[0]["rogl"].ToString(), null,
                                    dbamt, strrem, tbl.Rows[0]["parglcd"].ToString(), TTXN.SLCD, 0, 0, 0);
                            OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                            if (adrcr == "D") dbDrAmt = dbDrAmt + dbamt;
                            else dbCrAmt = dbCrAmt + dbamt;
                        }
                        //  Party wise posting
                        isl = 1; //isl + 1;
                        dbsql = masterHelp.InsVch_Det(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, Convert.ToSByte(isl), dr,
                            tbl.Rows[0]["parglcd"].ToString(), sslcd, Convert.ToDouble(VE.T_TXN.BLAMT), strrem, tbl.Rows[0]["prodglcd"].ToString(),
                            null, dbqty, 0, dbcurramt);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                        if (dr == "D") dbDrAmt = dbDrAmt + Convert.ToDouble(VE.T_TXN.BLAMT);
                        else dbCrAmt = dbCrAmt + Convert.ToDouble(VE.T_TXN.BLAMT);
                        dbsql = masterHelp.InsVch_Class(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, 1, Convert.ToSByte(isl), sslcd,
                                tbl.Rows[0]["class1cd"].ToString(), tbl.Rows[0]["class2cd"].ToString(), Convert.ToDouble(VE.T_TXN.BLAMT), dbcurramt, strrem);
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();

                        strblno = ""; strbldt = ""; strduedt = ""; strvtype = ""; strrefno = "";
                        if (VE.MENU_PARA == "PB" || VE.MENU_PARA == "SB" || VE.MENU_PARA == "SBPOS") { strvtype = "BL"; } else if (VE.MENU_PARA == "SD") { strvtype = "DN"; } else { strvtype = "CN"; }
                        strduedt = Convert.ToDateTime(TTXN.DOCDT.Value).AddDays(Convert.ToDouble(TTXN.DUEDAYS)).ToString().retDateStr();
                        if (VE.MENU_PARA == "PB")
                        {
                            strblno = TTXN.PREFNO;
                            strbldt = TTXN.PREFDT.ToString();
                        }
                        else
                        {
                            strbldt = TTXN.DOCDT.ToString();
                            strblno = DOCPATTERN;
                        }
                        string blconslcd = TTXN.CONSLCD;
                        if (TTXN.SLCD != sslcd) blconslcd = TTXN.SLCD;
                        if (blconslcd == sslcd) blconslcd = "";
                        dbsql = masterHelp.InsVch_Bl(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, dr,
                                tbl.Rows[0]["parglcd"].ToString(), sslcd, blconslcd, TTXNOTH.AGSLCD, tbl.Rows[0]["class1cd"].ToString(), Convert.ToSByte(isl),
                                VE.T_TXN.BLAMT.retDbl(), strblno, strbldt, strrefno, strduedt, strvtype, TTXN.DUEDAYS.retDbl(), itamt, TTXNOTH.POREFNO,
                                TTXNOTH.POREFDT == null ? "" : TTXNOTH.POREFDT.ToString().retDateStr(), VE.T_TXN.BLAMT.retDbl(),
                                "", "", "");
                        OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                    }

                    if (blgstpost == true)
                    {
                        #region TVCHGST Table update    

                        int gs = 0;
                        //  Posting of GST
                        double amthsnamt = 0, amtigst = 0, amtcgst = 0, amtsgst = 0, amtcess = 0, amtgstper = 0, mult = 1;
                        double rplamt = 0, rpligst = 0, rplcgst = 0, rplsgst = 0, rplcess = 0;
                        double balamt = 0, baligst = 0, balcgst = 0, balsgst = 0, balcess = 0;
                        double amtigstper = 0, amtcgstper = 0, amtsgstper = 0;

                        if (VE.TTXNAMT != null)
                        {
                            for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                            {
                                if (VE.TTXNAMT[i].AMT != 0 && VE.TTXNAMT[i].HSNCODE == null)
                                {
                                    if (VE.TTXNAMT[i].ADDLESS == "A") mult = 1; else mult = -1;
                                    amthsnamt = amthsnamt + (VE.TTXNAMT[i].AMT.retDbl() * mult);
                                    amtigst = amtigst + (VE.TTXNAMT[i].IGSTAMT.retDbl() * mult);
                                    amtcgst = amtcgst + (VE.TTXNAMT[i].CGSTAMT.retDbl() * mult);
                                    amtsgst = amtsgst + (VE.TTXNAMT[i].SGSTAMT.retDbl() * mult);
                                    amtcess = amtcess + (VE.TTXNAMT[i].CESSAMT.retDbl() * mult);
                                    amtigstper = VE.TTXNAMT[i].IGSTPER.retDbl();
                                    amtcgstper = VE.TTXNAMT[i].CGSTPER.retDbl();
                                    amtsgstper = VE.TTXNAMT[i].SGSTPER.retDbl();
                                }
                            }
                            if (amthsnamt != 0)
                            {
                                balamt = amthsnamt; baligst = amtigst; balcgst = amtcgst; balsgst = amtsgst; amtcess = balcess;
                                lastitemno = 0; titamt = 0;
                                for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
                                {
                                    if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null && VE.TTXNDTL[i].IGSTPER == amtigstper && VE.TTXNDTL[i].CGSTPER == amtcgstper && VE.TTXNDTL[i].SGSTPER == amtsgstper)
                                    {
                                        titamt = titamt + VE.TTXNDTL[i].AMT.retDbl();
                                        lastitemno = i;
                                    }
                                }

                                for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
                                {
                                    if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null && VE.TTXNDTL[i].IGSTPER == amtigstper && VE.TTXNDTL[i].CGSTPER == amtcgstper && VE.TTXNDTL[i].SGSTPER == amtsgstper)
                                    {
                                        if (i == lastitemno)
                                        {
                                            rplamt = balamt; rpligst = baligst; rplcgst = balcgst; rplsgst = balsgst; rplcess = balcess;
                                        }
                                        else
                                        {
                                            rplamt = ((amthsnamt / titamt) * VE.TTXNDTL[i].AMT).retDbl().toRound();
                                            rpligst = ((amtigst / titamt) * VE.TTXNDTL[i].AMT).retDbl().toRound();
                                            rplcgst = ((amtcgst / titamt) * VE.TTXNDTL[i].AMT).retDbl().toRound();
                                            rplsgst = ((amtsgst / titamt) * VE.TTXNDTL[i].AMT).retDbl().toRound();
                                            rplcess = ((amtcess / titamt) * VE.TTXNDTL[i].AMT).retDbl().toRound();
                                        }
                                        balamt = balamt - rplamt;
                                        baligst = baligst - rpligst;
                                        balcgst = balcgst - rplcgst;
                                        balsgst = balsgst - rplsgst;
                                        balcess = balcess - rplcess;

                                        VE.TTXNDTL[i].NETAMT = VE.TTXNDTL[i].NETAMT + rplamt;
                                        VE.TTXNDTL[i].IGSTAMT = VE.TTXNDTL[i].IGSTAMT + rpligst;
                                        VE.TTXNDTL[i].CGSTAMT = VE.TTXNDTL[i].CGSTAMT + rplcgst;
                                        VE.TTXNDTL[i].SGSTAMT = VE.TTXNDTL[i].SGSTAMT + rplsgst;
                                        VE.TTXNDTL[i].CESSAMT = VE.TTXNDTL[i].CESSAMT + rplcess;
                                    }
                                }
                            }
                        }

                        string dncntag = ""; string exemptype = "";
                        //if (VE.MENU_PARA == "SR" || VE.MENU_PARA == "SRPOS") dncntag = "SC";
                        //if (VE.MENU_PARA == "PR") dncntag = "PD";
                        double gblamt = TTXN.BLAMT.retDbl(); double groamt = TTXN.ROAMT.retDbl() + TTXN.TCSAMT.retDbl();
                        for (int i = 0; i <= VE.TTXNDTL.Count - 1; i++)
                        {
                            if (VE.TTXNDTL[i].SLNO != 0 && VE.TTXNDTL[i].ITCD != null)
                            {
                                gs = gs + 1;
                                if (VE.TTXNDTL[i].GSTPER.retDbl() == 0) exemptype = "N";
                                string SHIPDOCDT = VE.T_VCH_GST.SHIPDOCDT.retStr() == "" ? "" : VE.T_VCH_GST.SHIPDOCDT.retDateStr();
                                dbsql = masterHelp.InsVch_GST(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, cr,
                                        Convert.ToSByte(isl), Convert.ToSByte(gs), TTXN.SLCD, strblno, strbldt, VE.TTXNDTL[i].HSNCODE, VE.TTXNDTL[i].ITNM, (VE.TTXNDTL[i].BLQNTY == null || VE.TTXNDTL[i].BLQNTY == 0 ? VE.TTXNDTL[i].QNTY.retDbl() : VE.TTXNDTL[i].BLQNTY.retDbl()), (VE.TTXNDTL[i].UOM == null ? VE.TTXNDTL[i].UOM : VE.TTXNDTL[i].UOM),
                                        VE.TTXNDTL[i].NETAMT.retDbl(), VE.TTXNDTL[i].IGSTPER.retDbl(), VE.TTXNDTL[i].IGSTAMT.retDbl(), VE.TTXNDTL[i].CGSTPER.retDbl(), VE.TTXNDTL[i].CGSTAMT.retDbl(), VE.TTXNDTL[i].SGSTPER.retDbl(), VE.TTXNDTL[i].SGSTAMT.retDbl(),
                                        VE.TTXNDTL[i].CESSPER.retDbl(), VE.TTXNDTL[i].CESSAMT.retDbl(), salpur, groamt, gblamt, 0, (VE.T_VCH_GST.INVTYPECD == null ? "01" : VE.T_VCH_GST.INVTYPECD), VE.POS, VE.TTXNDTL[i].AGDOCNO, VE.TTXNDTL[i].AGDOCDT.ToString(), TTXNOTH.DNCNCD,
                                        VE.GSTSLNM, VE.GSTNO, VE.T_VCH_GST.EXPCD, VE.T_VCH_GST.SHIPDOCNO, SHIPDOCDT, VE.T_VCH_GST.PORTCD, dncntag, TTXN.CONSLCD, exemptype, 0, expglcd, "Y");
                                OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                gblamt = 0; groamt = 0;
                            }
                        }
                        if (VE.TTXNAMT != null)
                        {
                            for (int i = 0; i <= VE.TTXNAMT.Count - 1; i++)
                            {
                                if (VE.TTXNAMT[i].SLNO != 0 && VE.TTXNAMT[i].AMTCD != null && VE.TTXNAMT[i].AMT != 0 && VE.TTXNAMT[i].HSNCODE != null)
                                {
                                    gs = gs + 1;
                                    string SHIPDOCDT = VE.T_VCH_GST.SHIPDOCDT.retStr() == "" ? "" : VE.T_VCH_GST.SHIPDOCDT.retDateStr();
                                    dbsql = masterHelp.InsVch_GST(TTXN.AUTONO, TTXN.DOCCD, TTXN.DOCNO, TTXN.DOCDT.ToString(), TTXN.EMD_NO.Value, TTXN.DTAG, cr,
                                            Convert.ToSByte(isl), Convert.ToSByte(gs), TTXN.SLCD, strblno, strbldt, VE.TTXNAMT[i].HSNCODE, VE.TTXNAMT[i].AMTNM, 0, "OTH",
                                            VE.TTXNAMT[i].AMT.retDbl(), VE.TTXNAMT[i].IGSTPER.retDbl(), VE.TTXNAMT[i].IGSTAMT.retDbl(), VE.TTXNAMT[i].CGSTPER.retDbl(), VE.TTXNAMT[i].CGSTAMT.retDbl(), VE.TTXNAMT[i].SGSTPER.retDbl(), VE.TTXNAMT[i].SGSTAMT.retDbl(),
                                            VE.TTXNAMT[i].CESSPER.retDbl(), VE.TTXNAMT[i].CESSAMT.retDbl(), salpur, groamt, gblamt, 0, (VE.T_VCH_GST.INVTYPECD == null ? "01" : VE.T_VCH_GST.INVTYPECD), VE.POS, VE.TTXNDTL[0].AGDOCNO, VE.TTXNDTL[0].AGDOCDT.ToString(), TTXNOTH.DNCNCD,
                                            VE.GSTSLNM, VE.GSTNO, VE.T_VCH_GST.EXPCD, VE.T_VCH_GST.SHIPDOCNO, SHIPDOCDT, VE.T_VCH_GST.PORTCD, dncntag, TTXN.CONSLCD, exemptype, 0, VE.TTXNAMT[i].GLCD, "Y");
                                    OraCmd.CommandText = dbsql; OraCmd.ExecuteNonQuery();
                                    gblamt = 0; groamt = 0;
                                }
                            }
                        }
                        #endregion
                    }

                    if (Math.Round(dbDrAmt, 2) != Math.Round(dbCrAmt, 2))
                    {
                        dberrmsg = "Debit " + Math.Round(dbDrAmt, 2) + " & Credit " + Math.Round(dbCrAmt, 2) + " not matching please click on rounded off...";
                        goto dbnotsave;
                    }

                    #endregion
                    if (VE.DefaultAction == "A")
                    {
                        ContentFlg = "1" + " (Voucher No. " + DOCPATTERN + ")~" + TTXN.AUTONO;
                    }
                    else if (VE.DefaultAction == "E")
                    {
                        ContentFlg = "2";
                    }
                    OraTrans.Commit();
                    OraCon.Dispose();
                    return Content(ContentFlg);
                }
                else if (VE.DefaultAction == "V")
                {
                    dbsql = masterHelp.TblUpdt("t_txnoth", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_batchdtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_batchmst", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txnslsmn", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txnpymt", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txnamt", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                   
                    dbsql = masterHelp.TblUpdt("t_txn_linkno", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                   
                    dbsql = masterHelp.TblUpdt("t_txndtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_txn", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    dbsql = masterHelp.TblUpdt("t_cntrl_doc_pass", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc_dtl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_doc", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.TblUpdt("t_cntrl_hdr_rem", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    //dbsql = masterHelp.TblUpdt("t_cntrl_hdr_uniqno", VE.T_TXN.AUTONO, "D");
                    //dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }

                    //   Delete from financial schema
                    dbsql = masterHelp.TblUpdt("t_cntrl_auth", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); if (dbsql1.Count() > 1) { OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery(); }
                    dbsql = masterHelp.finTblUpdt("t_vch_gst", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_bl", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_class", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_det", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_vch_hdr", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();
                    dbsql = masterHelp.finTblUpdt("t_cntrl_hdr", VE.T_TXN.AUTONO, "D");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelp.T_Cntrl_Hdr_Updt_Ins(VE.T_TXN.AUTONO, "D", "S", null, null, null, VE.T_TXN.DOCDT.retStr(), null, null, null);
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery(); OraCmd.CommandText = dbsql1[1]; OraCmd.ExecuteNonQuery();


                    ModelState.Clear();
                    OraTrans.Commit();
                    OraCon.Dispose();
                    return Content("3");
                }
                else
                {
                    return Content("");
                }
                goto dbok;
                dbnotsave:;
                OraTrans.Rollback();
                return Content(dberrmsg);
                dbok:;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                OraTrans.Rollback();
                OraCon.Dispose();
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult Print(TransactionSalePosEntry VE, FormCollection FC, string DOCNO, string DOC_CD, string DOCDT)
        {
            try
            {
                ReportViewinHtml ind = new ReportViewinHtml();
                ind.DOCCD = DOC_CD;
                ind.FDOCNO = DOCNO;
                ind.TDOCNO = DOCNO;
                ind.FDT = DOCDT;
                ind.TDT = DOCDT;
                ind.MENU_PARA = "SALES";
                if (TempData["printparameter"] != null)
                {
                    TempData.Remove("printparameter");
                }
                TempData["printparameter"] = ind;
                return Content("");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult UploadImages(string ImageStr, string ImageName, string ImageDesc)
        {
            try
            {
                var extension = Path.GetExtension(ImageName);
                string filename = "I".retRepname() + extension;
                var link = Cn.SaveImage(ImageStr, "/UploadDocuments/" + filename);
                return Content("/UploadDocuments/" + filename);
            }
            catch (Exception ex)
            {
                return Content("//.");
            }

        }
        public Tuple<List<T_BATCH_IMG_HDR>> SaveBarImage(string BarImage, string BARNO, short EMD)
        {
            List<T_BATCH_IMG_HDR> doc = new List<T_BATCH_IMG_HDR>();
            int slno = 0;
            try
            {
                var BarImages = BarImage.retStr().Trim(Convert.ToChar(Cn.GCS())).Split(Convert.ToChar(Cn.GCS()));
                foreach (string image in BarImages)
                {
                    if (image != "")
                    {
                        var imagedes = image.Split('~');
                        T_BATCH_IMG_HDR mdoc = new T_BATCH_IMG_HDR();
                        mdoc.CLCD = CommVar.ClientCode(UNQSNO);
                        mdoc.EMD_NO = EMD;
                        mdoc.SLNO = Convert.ToByte(++slno);
                        mdoc.DOC_CTG = "PRODUCT";
                        var extension = Path.GetExtension(imagedes[0]);
                        mdoc.DOC_FLNAME = BARNO + "_" + slno + extension;
                        mdoc.DOC_DESC = imagedes[1].retStr().Replace('~', ' ');
                        mdoc.BARNO = BARNO;
                        mdoc.DOC_EXTN = extension;
                        doc.Add(mdoc);
                        string topath = CommVar.SaveFolderPath() + "/ItemImages/" + mdoc.DOC_FLNAME;
                        topath = Path.Combine(topath, "");
                        string frompath = System.Web.Hosting.HostingEnvironment.MapPath("/UploadDocuments/" + imagedes[0]);
                        Cn.CopyImage(frompath, topath);
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, BarImage);
            }
            var result = Tuple.Create(doc);
            return result;
        }
    }
}