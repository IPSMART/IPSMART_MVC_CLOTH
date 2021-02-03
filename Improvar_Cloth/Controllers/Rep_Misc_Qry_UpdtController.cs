﻿using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;

namespace Improvar.Controllers
{
    public class Rep_Misc_Qry_UpdtController : Controller
    {
        // GET: Rep_Misc_Qry_Updt
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string fdt = ""; string tdt = ""; bool showpacksize = false, showrate = false;
        string modulecode = CommVar.ModuleCode();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult Rep_Misc_Qry_Updt()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Misc Update Queries";
                    RepMiscQryUpdt VE = new RepMiscQryUpdt();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    string com = CommVar.Compcd(UNQSNO); string gcs = Cn.GCS();
                    List<DropDown_list1> CHNGSTYL = new List<DropDown_list1>();
                    CHNGSTYL.Add(new DropDown_list1 { value = "Change Style", text = "Change Style No in Bale" });
                    CHNGSTYL.Add(new DropDown_list1 { value = "Change Pageno", text = "Change Pageno in bale" });
                    VE.DropDown_list1 = CHNGSTYL;
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
        public ActionResult GetBaleNoDetails(string val, string BLSLNO = "")
        {
            try
            {
               
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                var tdt = CommVar.CurrDate(UNQSNO);
                if (val == null)
                {
                    return PartialView("_Help2", MasterHelp.BaleNo_help(val, tdt, BLSLNO));
                }
                else
                { string sql = "select distinct BALENO,BALEYR,BLSLNO from " + CommVar.CurSchema(UNQSNO) + ".T_BALE where BALENO='" + val + "' ";
                    if (BLSLNO.retStr() != "") sql += "and BLSLNO='" + BLSLNO + "' ";
                    DataTable dt = MasterHelp.SQLquery(sql);
                  
                    if(dt.Rows.Count>1)
                    { return Content("");
                    }
                    else {
                        var balenoyr = (from DataRow dr in dt.Rows
                                        select new
                                        {
                                            BALENO = dr["BALENO"].retStr() + dr["BALEYR"].retStr(),
                                            BALESLNO = dr["BLSLNO"].retStr()
                                    }).FirstOrDefault();
                        val = balenoyr.BALENO;
                        BLSLNO = balenoyr.BALESLNO;


                    }
                    string str = MasterHelp.BaleNo_help(val, tdt, BLSLNO);
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
                //sequence MTRLJOBCD/PARTCD/DOCDT/TAXGRPCD/GOCD/PRCCD/ALLMTRLJOBCD
                Cn.getQueryString(VE);
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                string barnoOrStyle = val.retStr();
                string MTRLJOBCD = data[0].retSqlformat();
                string PARTCD = data[1].retStr();
                string DOCDT = data[2].retStr();
                string TAXGRPCD = data[3].retStr();
                string GOCD = data[2].retStr() == "" ? "" : data[4].retStr().retSqlformat();
                string PRCCD = data[5].retStr();
                if (MTRLJOBCD == "" || barnoOrStyle == "") { MTRLJOBCD = data[6].retStr(); }
                string BARNO = data[8].retStr() == "" || val.retStr() == "" ? "" : data[8].retStr().retSqlformat();
                bool exactbarno = data[7].retStr() == "Bar" ? true : false;

                if (GOCD == "")
                {
                    return Content("Please fill Godown");
                }
                string str = MasterHelp.T_TXN_BARNO_help(barnoOrStyle, VE.MENU_PARA, DOCDT, TAXGRPCD, GOCD, PRCCD, MTRLJOBCD, "", exactbarno, PARTCD, BARNO);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (str.IndexOf(Cn.GCS()) == -1) return Content(str);
                    string glcd = "";
                    glcd = str.retCompValue("SALGLCD");

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
        public ActionResult Save(RepMiscQryUpdt VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string dbsql = "", sql = "", query = "";
            Int16 emdno = 1;
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            OracleTransaction OraTrans;
            OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            OraCmd.Transaction = OraTrans;
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                    string ContentFlg = "";
                    var schnm = CommVar.CurSchema(UNQSNO);
                    var CLCD = CommVar.ClientCode(UNQSNO);
                    if (VE.TEXTBOX1 == "Change Style")
                    {
                        string sql1 = "select * from " + schnm + ". M_SITEM where STYLENO='" + VE.OLDSTYLENO + "' ";
                        DataTable dt = MasterHelp.SQLquery(sql1);
                        var itcd = (from DataRow dr in dt.Rows select new { itcd = dr["itcd"]    
                        }).FirstOrDefault();
                
                        sql = "update " + schnm + ". M_SITEM set STYLENO= '" + VE.NEWSTYLENO + "', CLCD='" + CLCD + "' "
                    + " where ITCD='" + itcd.itcd + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". T_TXNDTL set  BALENO= '" + VE.BALENO1 + "',BALEYR= '" + VE.BALEYR1 + "', CLCD='" + CLCD + "' "
                        + " where AUTONO='" + VE.BLAUTONO1 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". T_BATCHDTL set  BALENO= '" + VE.BALENO1 + "',BALEYR= '" + VE.BALEYR1 + "', CLCD='" + CLCD + "' "
                            + " where AUTONO='" + VE.BLAUTONO1 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". T_BILTY set  BALENO='" + VE.BALENO1 + "',BALEYR= '" + VE.BALEYR1 + "',LRDT= to_date('" + VE.LRDT1 + "','dd/mm/yyyy'),LRNO= '" + VE.LRNO1 + "', CLCD='" + CLCD + "' "
                       + " where BLAUTONO= '" + VE.BLAUTONO1 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();


                        sql = "update " + schnm + ". T_BALE set BALENO='" + VE.BALENO1 + "', BALEYR= '" + VE.BALEYR1 + "',BLSLNO= '" + VE.BLSLNO1 + "',LRDT= to_date('" + VE.LRDT1 + "','dd/mm/yyyy'),LRNO= '" + VE.LRNO1 + "', CLCD='" + CLCD + "' "
                          + " where BLAUTONO= '" + VE.BLAUTONO1 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                        

                    }
                    else
                    {
                        sql = "update " + schnm + ". T_BALE set  BALENO='" + VE.BALENO2 + "',BALEYR= '" + VE.BALEYR2 + "',BLSLNO= '" + VE.BLSLNO2 + "',LRDT= to_date('" + VE.LRDT2 + "','dd/mm/yyyy'),LRNO= '" + VE.LRNO2 + "', CLCD='" + CLCD + "' "
                        + " where BLAUTONO= '" + VE.BLAUTONO2 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". T_BILTY set  BALENO='" + VE.BALENO2 + "',BALEYR= '" + VE.BALEYR2 + "',LRDT= to_date('" + VE.LRDT2 + "','dd/mm/yyyy'),LRNO= '" + VE.LRNO2 + "', CLCD='" + CLCD + "' "
                              + " where BLAUTONO= '" + VE.BLAUTONO2 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". T_BATCHDTL set BALENO='" + VE.BALENO2 + "', BALEYR= '" + VE.BALEYR2 + "', CLCD='" + CLCD + "' "
                              + " where  AUTONO= '" + VE.BLAUTONO2 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ". T_TXNDTL set BALENO='" + VE.BALENO2 + "', BALEYR= '" + VE.BALEYR2 + "', CLCD='" + CLCD + "',PAGENO='" + VE.NEWPAGENO + "',PAGESLNO='" + VE.NEWPAGESLNO + "'  "
                        + " where AUTONO='" + VE.BLAUTONO2 + "'  ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    }
                    ModelState.Clear();
                    transaction.Commit();
                    OraTrans.Commit();
                    OraTrans.Dispose();
                    ContentFlg = "1";
                    return Content(ContentFlg);
                }
                catch (Exception ex)
                {
                    DB.SaveChanges();
                    transaction.Rollback();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
    }
}