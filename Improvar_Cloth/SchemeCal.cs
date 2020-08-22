﻿using System;
using System.Data;

namespace Improvar
{
    public class SchemeCal : MasterHelpFa
    {
        string CS = null;
        Connection Cn = new Connection();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public string GenScmItmGrpString(String scmitmgrpcd, string selitcd = "", string OnlyCode="N", bool skipsize = false)
        {
           
            try
            {
                string scm =  CommVar.CurSchema(UNQSNO);
                string sql = "";
                string scmgrpcode = scmitmgrpcd.Replace("'","").Replace(",","");

                if (OnlyCode == "N")
                {
                    sql += "select '" + scmgrpcode + "' scmitmgrpcd, a.brandcd, b.brandnm, a.sbrandcd, c.sbrandnm, a.collcd, d.collnm, a.itgrpcd, e.itgrpnm, ";
                    sql += "a.styleno, f.itnm, a.itcd, a.sizecd, g.print_seq, g.sizenm, a.colrcd, h.colrnm, ";
                    sql += "a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') itcolsize ";
                }
                else
                {
                    sql += "select '" + scmgrpcode + "' scmitmgrpcd, a.itcd||nvl(a.colrcd,'')||nvl(a.sizecd,'') itcolsize ";
                }
                sql += "from ( ";
                sql += "select distinct a.itcd, d.brandcd, a.sbrandcd, a.itgrpcd, a.collcd, a.styleno, ";
                if (skipsize == true) sql += "'' sizecd, '' colrcd "; else sql += "b.sizecd, c.colrcd ";
                sql += "from " + scm + ".m_sitem a, " + scm + ".m_sitem_size b, " + scm + ".m_sitem_color c, " + scm + ".m_group d ";
                sql += "where a.itcd=b.itcd(+) and a.itcd=c.itcd(+) and a.itgrpcd=d.itgrpcd(+) and ";
                sql += "( ";
                sql += "(d.brandcd in (select r.brandcd from " + scm + ".m_scmitmgrp r ";
                sql += "where r.scmitmgrpcd in (" + scmitmgrpcd + ") and sbrandcd||collcd||itgrpcd||itcd is null)) or ";
                sql += "(d.brandcd||a.sbrandcd in (select r.brandcd||r.sbrandcd from " + scm + ".m_scmitmgrp r ";
                sql += "where r.scmitmgrpcd in (" + scmitmgrpcd + ") and collcd||itgrpcd||itcd is null)) or ";
                sql += "(d.brandcd||a.sbrandcd||a.collcd in (select r.brandcd||r.sbrandcd||r.collcd from " + scm + ".m_scmitmgrp r ";
                sql += "where r.scmitmgrpcd in (" + scmitmgrpcd + ") and itgrpcd||itcd is null)) or ";
                //sql += "(d.brandcd||a.sbrandcd||a.collcd||a.itgrpcd in (select r.brandcd||r.sbrandcd||r.collcd||r.itgrpcd from " + scm + ".m_scmitmgrp r ";
                //sql += "where r.scmitmgrpcd in (" + scmitmgrpcd + ") and itcd is null)) or ";
                sql += "(d.brandcd||a.itgrpcd in (select r.brandcd||r.itgrpcd from " + scm + ".m_scmitmgrp r ";
                sql += "where r.scmitmgrpcd in (" + scmitmgrpcd + ") and sbrandcd||collcd||itcd is null)) or ";
                sql += "(d.brandcd||a.sbrandcd||a.collcd||a.itgrpcd||a.itcd in (select t.brandcd||s.sbrandcd||s.collcd||s.itgrpcd||r.itcd ";
                sql += "from " + scm + ".m_scmitmgrp r, " + scm + ".m_sitem s, " + scm + ".m_group t ";
                sql += "where r.scmitmgrpcd in (" + scmitmgrpcd + ") and r.itcd=s.itcd(+) and s.itgrpcd=t.itgrpcd(+) and r.itcd is not null and r.sizecd||r.colrcd is null)) or ";
                sql += "(d.brandcd||a.sbrandcd||a.collcd||a.itgrpcd||a.itcd||b.sizecd||c.colrcd in (select t.brandcd||s.sbrandcd||s.collcd||s.itgrpcd||r.itcd||r.sizecd||r.colrcd  ";
                sql += "from " + scm + ".m_scmitmgrp r, " + scm + ".m_sitem s, " + scm + ".m_group t ";
                sql += "where r.scmitmgrpcd in (" + scmitmgrpcd + ") and r.itcd=s.itcd(+) and s.itgrpcd=t.itgrpcd(+) and r.itcd||r.sizecd||r.colrcd is not null)) or ";
                sql += "(d.brandcd||a.sbrandcd||a.collcd||a.itgrpcd||a.itcd||b.sizecd in (select t.brandcd||s.sbrandcd||s.collcd||s.itgrpcd||r.itcd||r.sizecd ";
                sql += "from " + scm + ".m_scmitmgrp r, " + scm + ".m_sitem s, " + scm + ".m_group t ";
                sql += "where r.scmitmgrpcd in (" + scmitmgrpcd + ") and r.itcd=s.itcd(+) and s.itgrpcd=t.itgrpcd(+) and r.itcd||r.sizecd is not null)) or ";
                sql += "(d.brandcd||a.sbrandcd||a.collcd||a.itgrpcd||a.itcd||c.colrcd in (select t.brandcd||s.sbrandcd||s.collcd||s.itgrpcd||r.itcd||r.colrcd ";
                sql += "from " + scm + ".m_scmitmgrp r, " + scm + ".m_sitem s, " + scm + ".m_group t ";
                sql += "where r.scmitmgrpcd in (" + scmitmgrpcd + ") and r.itcd=s.itcd(+) and s.itgrpcd=t.itgrpcd(+) and r.itcd||r.colrcd is not null)) ";
                sql += ") ";
                sql += ") a, " + scm + ".m_brand b, " + scm + ".m_subbrand c, " + scm + ".m_collection d, " + scm + ".m_group e, ";
                sql += "" + scm + ".m_sitem f, " + scm + ".m_size g, " + scm + ".m_color h ";
                sql += "where a.brandcd=b.brandcd(+) and a.sbrandcd=c.sbrandcd(+) and a.collcd=d.collcd(+) and a.itgrpcd=e.itgrpcd(+) and ";
                sql += "a.itcd=f.itcd(+) and a.sizecd=g.sizecd(+) and a.colrcd=h.colrcd(+) ";
                if (selitcd != "") sql += "and a.itcd in (" + selitcd + ") ";
                sql += "order by brandnm, brandcd, sbrandnm, sbrandcd, collnm, collcd, itgrpnm, itgrpcd, ";
                sql += "styleno, print_seq, sizenm, sizecd, colrnm, colrcd ";

                return sql;
            }
            catch (Exception EX)
            {
                return "";
            }
        }
        public DataTable GenScmItmGrpData(String scmitmgrpcd, string selitcd = "", bool skipsize=false)
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            DataTable dtScmCd = new DataTable();
            try
            {
                string scm =  CommVar.CurSchema(UNQSNO);
                string sql = "";

                sql = GenScmItmGrpString(scmitmgrpcd, selitcd, "N", skipsize);

                dtScmCd = SQLquery(sql);
                return dtScmCd;
            }
            catch (Exception EX)
            {
                return dtScmCd;
            }          
        }

        //public DataTable GetSchmeCode(string docdt, string slcd = "", string brandcd = "")
        //{
        //    DataTable dtScmCd = new DataTable();
        //    try
        //    {
        //        string scm =  CommVar.CurSchema(UNQSNO);
        //        string sql = "";

        //        sql += "select distinct a.scmcd, a.scmnm, a.scmdt, a.scmbasis, a.frdt, a.todt ";
        //        if (slcd == "") sql += ", d.slcd ";
        //        sql += "from " + scm + ".m_scheme_hdr a, " + scm + ".m_scheme_slcd b, " + scm + ".m_cntrl_hdr c, ";
        //        sql += scm + ".m_subleg_com d, " + scm + ".m_subleg_brand e ";
        //        sql += "where a.scmcd=b.scmcd(+) and a.m_autono=c.m_autono(+) and ";
        //        sql += "(to_date('" + docdt + "','dd/mm/yyyy') between a.frdt and a.todt) and ";
        //        sql += "nvl(c.inactive_tag,'N')='N' and ";
        //        sql += "d.slcd=e.slcd(+) and (e.brandcd is null ";
        //        if (brandcd != "") sql += "or e.brandcd in(" + brandcd + ")) ";
        //        sql += " and ";
        //        if (slcd != "") sql += "d.slcd in (" + slcd + ") and ";
        //        sql += "(b.agslcd=nvl(e.agslcd,d.agslcd) or b.areacd=d.areacd or b.slcd=d.slcd or b.areacd||b.agslcd||b.slcd is null) ";

        //        sql = GenScmItmGrpString(sql);

        //        dtScmCd = SQLquery(sql);

        //        return dtScmCd;
        //    }
        //    catch (Exception EX)
        //    {
        //        return dtScmCd;
        //    }
        //}

        public DataTable GetSchmeCode(string docdt, string slcd = "", string brandcd = "")
        {
            
            DataTable dtScmCd = new DataTable();
            try
            {
                string scm =  CommVar.CurSchema(UNQSNO);
                string sql = "";
                if (slcd != "") slcd = "'" + slcd + "'";
                if (brandcd != "") brandcd = "'" + brandcd + "'";
                sql += "select distinct a.scmcd, a.scmnm, a.scmdt, a.scmtype,a.scmbasis, a.frdt, a.todt ";
                if (slcd == "") sql += ", d.slcd ";
                sql += "from " + scm + ".m_scheme_hdr a, " + scm + ".m_scheme_slcd b, " + scm + ".m_cntrl_hdr c, ";
                sql += scm + ".m_subleg_com d, " + scm + ".m_subleg_brand e ";
                sql += "where a.scmcd=b.scmcd(+) and a.m_autono=c.m_autono(+) and ";
                sql += "(to_date('" + docdt + "','dd/mm/yyyy') between a.frdt and a.todt) and ";
                sql += "nvl(c.inactive_tag,'N')='N' and ";
                sql += "d.slcd=e.slcd(+) and ";
                if (brandcd != "") sql += "(e.brandcd is null or e.brandcd in(" + brandcd + ")) and ";
                if (slcd != "") sql += "d.slcd in (" + slcd + ") and ";
                sql += "(b.agslcd=nvl(e.agslcd,d.agslcd) or b.areacd=d.areacd or b.slcd=d.slcd or b.areacd||b.agslcd||b.slcd is null) ";
                
                dtScmCd = SQLquery(sql);

                return dtScmCd;
            }
            catch (Exception EX)
            {
                return dtScmCd;
            }
        }
    }
}