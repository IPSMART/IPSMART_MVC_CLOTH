﻿using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using OfficeOpenXml;
using System.Collections.Generic;
using OfficeOpenXml.Style;

namespace Improvar.Controllers
{
    public class Rep_Bale_HistoryController : Controller
    {
        // GET: Rep_Bale_History
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string fdt = ""; string tdt = ""; bool showpacksize = false, showrate = false;
        string modulecode = CommVar.ModuleCode();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult Rep_Bale_History()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Bale Report";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    string com = CommVar.Compcd(UNQSNO); string gcs = Cn.GCS();
                    string qry = "select distinct baleno ||'/'|| baleyr BaleNoBaleYr,baleno || baleyr BaleNoBaleYrcd from " + CommVar.CurSchema(UNQSNO) + ".t_txndtl where  baleno is not null and baleyr is not null  ";
                    DataTable tbl = MasterHelp.SQLquery(qry);
                    VE.DropDown_list1 = (from DataRow dr in tbl.Rows select new DropDown_list1() { value = dr["BaleNoBaleYrcd"].retStr(), text = dr["BaleNoBaleYr"].retStr() }).OrderBy(s => s.text).ToList();
                    VE.TEXTBOX1 = MasterHelp.ComboFill("BaleNoBaleYrcd", VE.DropDown_list1, 0, 1);
                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);
                    VE.DropDown_list_ITGRP = DropDownHelp.GetItgrpcdforSelection();
                    VE.Itgrpnm = MasterHelp.ComboFill("itgrpcd", VE.DropDown_list_ITGRP, 0, 1);

                    VE.FDT = CommVar.FinStartDate(UNQSNO); VE.TDT = CommVar.CurrDate(UNQSNO);
                    VE.Checkbox1 = true;
                    VE.DefaultView = true;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }

        [HttpPost]
        public ActionResult Rep_Bale_History(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                fdt = VE.FDT.retDateStr(); tdt = VE.TDT.retDateStr();


                string txntag = ""; string txnrettag = "", itcd = "";
                //string reptype = FC["Reptype"].ToString();
                //string repon = FC["PartyItem"].ToString();
                //string grpcd = VE.TEXTBOX2;
                //string groupingwith = VE.TEXTBOX3;
                //bool WithoutParty = VE.Checkbox3;
                string BalenoBaleyr = "";
                string selitcd = "", unselitcd = "", plist = "", selslcd = "", unselslcd = "", selitgrpcd = "", selbrgrpcd = "";
                if (FC.AllKeys.Contains("BaleNoBaleYrcdvalue")) BalenoBaleyr = CommFunc.retSqlformat(FC["BaleNoBaleYrcdvalue"].ToString());

                if (FC.AllKeys.Contains("itcdvalue")) itcd = FC["itcdvalue"].retSqlformat();
                if (FC.AllKeys.Contains("itgrpcdvalue")) selitgrpcd = CommFunc.retSqlformat(FC["itgrpcdvalue"].ToString());
                txntag = txntag + txnrettag;
                bool RepeatAllRow = VE.Checkbox1;

                string sql = "";
                sql += "select a.autono, a.txnslno, a.gocd, a.stkdrcr, a.baleno, a.baleyr,a.baleno || a.baleyr BaleNoBaleYrcd, a.itcd, a.shade, a.nos, a.qnty, c.usr_entdt, ";
                sql += "c.docno, c.docdt, d.prefno, d.slcd, h.slnm, g.gonm, f.styleno, f.itnm, f.itgrpcd, f.uomcd, c.doccd, e.docnm, e.doctype, ";
                sql += "b.pageno, b.pageslno, b.lrno from ";
                sql += "(select a.autono, a.txnslno, a.autono || a.txnslno autoslno, a.gocd, b.stkdrcr, b.baleno, b.baleyr, b.itcd, a.shade, c.slcd, ";
                sql += "sum(a.nos) nos, sum(a.qnty) qnty ";
                sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_txndtl b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d ";
                sql += "where a.autono = b.autono(+) and a.txnslno = b.slno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and ";
                sql += "d.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and d.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and ";
                sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and a.baleno is not null ";
                sql += "group by a.autono, a.txnslno, a.autono || a.txnslno, a.gocd, b.stkdrcr, b.baleno, b.baleyr, b.itcd, a.shade, c.slcd ";
                sql += "union all ";
                sql += "select e.autono, a.txnslno, a.autono || a.txnslno autoslno, a.gocd, b.stkdrcr, b.baleno, b.baleyr, b.itcd, a.shade, f.mutslcd slcd, ";
                sql += "sum(a.nos) nos, sum(a.qnty) qnty ";
                sql += "from " + scm + ".t_bilty e, " + scm + ".t_batchdtl a, " + scm + ".t_txndtl b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".t_bilty_hdr f, " + scm + ".m_doctype g ";
                sql += "where e.blautono = a.autono(+) and e.baleno = a.baleno(+) and e.baleyr = a.baleyr(+) and ";
                sql += "a.autono = b.autono(+) and a.txnslno = b.slno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and ";
                sql += "d.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and d.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and ";
                sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and a.baleno is not null and e.autono = f.autono(+) and g.doctype not in ('KHSR') ";
                sql += "group by e.autono, a.txnslno, a.autono || a.txnslno, a.gocd, b.stkdrcr, b.baleno, b.baleyr, b.itcd, a.shade, f.mutslcd ";
                sql += "union all ";
                sql += "select e.autono, a.txnslno, a.autono || a.txnslno autoslno, a.gocd, b.stkdrcr, b.baleno, b.baleyr, b.itcd, a.shade, f.mutslcd slcd, ";
                sql += "sum(a.nos) nos, sum(a.qnty) qnty ";
                sql += "from " + scm + ".t_bale e, " + scm + ".t_batchdtl a, " + scm + ".t_txndtl b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".t_bale_hdr f, " + scm + ".m_doctype g ";
                sql += "where e.blautono = a.autono(+) and e.baleno = a.baleno(+) and e.baleyr = a.baleyr(+) and ";
                sql += "a.autono = b.autono(+) and a.txnslno = b.slno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and ";
                sql += "d.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and d.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and ";
                sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and a.baleno is not null and e.autono = f.autono(+) and g.doctype not in ('KHSR') ";
                sql += "group by e.autono, a.txnslno, a.autono || a.txnslno, a.gocd, b.stkdrcr, b.baleno, b.baleyr, b.itcd, a.shade, f.mutslcd ) a, ";

                sql += "(select a.autono, a.slno, a.autono || a.slno autoslno, c.lrno, a.pageno, a.pageslno ";
                sql += " from " + scm + ".t_txndtl a, " + scm + ".t_txn b, " + scm + ".t_txntrans c ";
                sql += "where a.autono = b.autono(+) and a.autono = c.autono(+) and nvl(a.pageno, 0) <> 0 ) b, ";

                sql += "" + scm + ".t_cntrl_hdr c, " + scm + ".t_txn d, " + scm + ".m_doctype e, ";
                sql += "" + scm + ".m_sitem f, " + scmf + ".m_godown g, " + scmf + ".m_subleg h ";
                sql += "where a.autoslno = b.autoslno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and ";
                sql += "c.doccd = e.doccd(+) and a.itcd = f.itcd(+) and a.gocd = g.gocd(+) and a.slcd = h.slcd(+) ";
                if (BalenoBaleyr.retStr() != "") sql += "and a.baleno || a.baleyr in(" + BalenoBaleyr + ")";
                if (itcd.retStr() != "") sql += "and a.itcd in(" + itcd + ")";
                if (selitgrpcd.retStr() != "") sql += "and f.itgrpcd in(" + selitgrpcd + ")";
                sql += "order by baleyr, baleno, styleno, usr_entdt ";


                DataTable tbl = MasterHelp.SQLquery(sql);
                if (tbl.Rows.Count == 0) return Content("no records..");

                return ReportHistory(tbl, RepeatAllRow, VE.Checkbox2, VE.Checkbox6);


            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public ActionResult ReportHistory(DataTable tbl, bool RepeatAllRow, bool batchprint, bool monthtotal)
        {
            try
            {
                Int32 i = 0;
                Int32 maxR = 0;
                string chkval, chkval1 = "", chkval2 = "";
                if (tbl.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer");
                }

                DataTable IR = new DataTable("mstrep");

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                HC.RepStart(IR, 3);
                HC.GetPrintHeader(IR, "baleno", "string", "c,10", "Bale No");
                HC.GetPrintHeader(IR, "styleno", "string", "c,25", "Style No");
                HC.GetPrintHeader(IR, "pblno", "string", "c,20", "P/Blno");
                HC.GetPrintHeader(IR, "lrno", "string", "c,10", "LR No");
                HC.GetPrintHeader(IR, "docnm", "string", "c,10", "Activity In");
                HC.GetPrintHeader(IR, "docdt", "string", "c,16", "Doc Date");
                HC.GetPrintHeader(IR, "docno", "string", "c,16", "Doc No");
                HC.GetPrintHeader(IR, "gonm", "string", "c,16", "Godown");
                HC.GetPrintHeader(IR, "slnm", "string", "c,16", "Particulars");
                HC.GetPrintHeader(IR, "nos", "string", "c,16", "Nos");
                HC.GetPrintHeader(IR, "qnty", "string", "c,16", "Qnty");

                double qty, amt = 0;
                double tqty = 0, tnos = 0;
                double gtqty = 0, gtnos = 0;

                Int32 rNo = 0;
                string baleno = "";
                // Report begins
                i = 0; maxR = tbl.Rows.Count - 1;
                int count = 0;
                while (i <= maxR)
                {
                    chkval = tbl.Rows[i]["BaleNoBaleYrcd"].ToString();
                    qty = 0; amt = 0;
                    bool balefirst = true;
                    //IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    //IR.Rows[rNo]["cd"] = tbl.Rows[i]["cd"].ToString();
                    //IR.Rows[rNo]["nm"] = tbl.Rows[i]["nm"].ToString();
                    //IR.Rows[rNo]["snm"] = tbl.Rows[i]["snm"].ToString();
                    //IR.Rows[rNo]["celldesign"] = "cd=font-weight:bold;font-size:13px;^nm=font-weight:bold;font-size:13px;^snm=font-weight:bold;font-size:13px; ";
                    while (tbl.Rows[i]["BaleNoBaleYrcd"].ToString() == chkval)
                    {
                        bool itemfirst = true;
                        baleno = tbl.Rows[i]["baleno"].ToString();

                        chkval2 = tbl.Rows[i]["itcd"].ToString();
                        while (tbl.Rows[i]["itcd"].ToString() == chkval2)
                        {
                            tnos = tnos + tbl.Rows[i]["qnty"].retDbl();
                            tqty = tqty + tbl.Rows[i]["qnty"].retDbl();

                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            if (RepeatAllRow == true || balefirst == true ) IR.Rows[rNo]["baleno"] = tbl.Rows[i]["baleno"].retStr();
                            if (RepeatAllRow == true || itemfirst ==true) IR.Rows[rNo]["styleno"] = tbl.Rows[i]["styleno"].ToString();
                            IR.Rows[rNo]["pblno"] = tbl.Rows[i]["prefno"].ToString();
                            IR.Rows[rNo]["lrno"] = tbl.Rows[i]["lrno"].ToString();
                            IR.Rows[rNo]["docnm"] = tbl.Rows[i]["docnm"].ToString();
                            IR.Rows[rNo]["docdt"] = Convert.ToString(tbl.Rows[i]["docdt"]).Substring(0, 10);
                            IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].ToString();
                            IR.Rows[rNo]["gonm"] = tbl.Rows[i]["gonm"].ToString();
                            IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"].ToString();
                            IR.Rows[rNo]["nos"] = tbl.Rows[i]["nos"].ToString();
                            IR.Rows[rNo]["qnty"] = tbl.Rows[i]["qnty"].ToString();
                            balefirst = false; itemfirst = false;
                            i = i + 1;
                            if (i > maxR) break;
                        }

                        count++;
                        if (i > maxR) break;
                    }
                    if (RepeatAllRow == false)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["dammy"] = "";
                        //IR.Rows[rNo]["baleno"] = "Total of Bale " + baleno + " ";
                        IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                        //IR.Rows[rNo]["nos"] = tnos;
                        //IR.Rows[rNo]["qnty"] = tqty;
                    }

                    gtqty = gtqty + tnos;
                    gtnos = gtnos + tqty;
                }
                // Create Blank line
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dammy"] = " ";
                IR.Rows[rNo]["flag"] = " height:14px; ";

                string pghdr1 = " Bale History from " + fdt + " to " + tdt;
                string repname = "Bale Report";
                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "L", false);

                TempData[repname] = PV;
                TempData[repname + "xxx"] = IR;
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }

    }
}