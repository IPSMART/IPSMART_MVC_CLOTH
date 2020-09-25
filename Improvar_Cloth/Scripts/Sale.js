﻿
    function GetAllMtrljobcd() {
        if ("@Model.DefaultAction" != "V") {
            var GridRowMain = $("#exampleLT > tbody > tr").length;
            var allcode = "";
            var mtrljobcd = $("#MTRLJOBCD").val();
            if (mtrljobcd != "") {
                allcode += "'" + mtrljobcd + "'";
            }
            for (j = 0; j <= GridRowMain - 1; j++) {
                if (document.getElementById("MaterialJobChk_" + j).checked == true) {
                    if (allcode == "") {
                        allcode += "'" + $("#MaterialJobCode_" + j).val() + "'";
                    }
                    else {
                        allcode += ",'" + $("#MaterialJobCode_" + j).val() + "'";
                    }
                }
            }
            $("#ALLMTRLJOBCD").val(allcode);
        }
    }
function GetBarnoDetails(id) {
    if ("@Model.DefaultAction" != "V") {
        debugger;
        if (id == "") {
            ClearBarcodeArea();
        }
        else {
            if (!emptyFieldCheck("Please Select / Enter Document Date", "DOCDT")) { return false; }
            if ($("#TAXGRPCD").val() == "") { $("#BARCODE").val(""); msgInfo("TaxGrp. Code not available.Please Select / Enter another Party Code to get TaxGrp. Code"); message_value = "SLCD"; return false; }
            if ($("#PRCCD").val() == "") { $("#BARCODE").val(""); msgInfo("Price Code not available.Please Select / Enter another Party Code to get Price Code"); message_value = "SLCD"; return false; }
            var docdt = $("#DOCDT").val();
            var taxgrpcd = $("#TAXGRPCD").val();
            var gocd = $("#GOCD").val();
            var prccd = $("#PRCCD").val();
            var mtrljobcd = $("#ALLMTRLJOBCD").val();
            var code = docdt + String.fromCharCode(181) + taxgrpcd + String.fromCharCode(181) + gocd + String.fromCharCode(181) + prccd + String.fromCharCode(181) + mtrljobcd;
            $.ajax({
                type: 'POST',
                beforesend: $("#WaitingMode").show(),
                url: "@Url.Action("GetBarCodeDetails", PageControllerName)",
                data: "&val=" + id + "&Code=" + code,
            success: function (result) {
                var MSG = result.indexOf('id="_T_SALE_BARNODETAIL_GRID"');
                if (MSG >= 0) {
                    $("#popup").animate({ marginTop: '-10px' }, 50);
                    $("#popup").html(result);
                    $("#WaitingMode").hide();
                }
                else {
                    var MSG = result.indexOf(String.fromCharCode(181));
                    if (MSG >= 0) {
                        FillBarcodeArea(result);
                    }
                    else {
                        $('#helpDIV').html("");
                        msgInfo("" + result + " !");
                        ClearBarcodeArea();
                        message_value = "BARCODE";
                    }
                }
                $("#WaitingMode").hide();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $("#WaitingMode").hide();
                msgError(XMLHttpRequest.responseText);
                $("body span h1").remove(); $("#msgbody_error style").remove();
            }
        });
    }
}
}
function Add_BarCodeRow() {
    if ("@Model.DefaultAction" != "V") {
        $.ajax({
            type: 'POST',
            beforesend: $("#WaitingMode").show(),
            url: "@Url.Action("FillBarCodeData", PageControllerName)",
            data: $('form').serialize(),
        success: function (result) {
            //$("#partialdivBarCodeTab").animate({ marginTop: '-10px' }, 50);
            $("#partialdivBarCodeTab").html(result);
            CalculateTotal_Barno();
            $("#WaitingMode").hide();
            ClearBarcodeArea();
            @*if ("@Model.MENU_PARA" == "PB") {
                $("#BALENO").val("");
            }
            $("#DISCTYPE").val("P");
            $("#TDDISCTYPE").val("P");
            $("#SCMDISCTYPE").val("P");*@
            },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
}
}
function FillBarcodeArea(str, Table, i) {
    if ("@Model.DefaultAction" != "V") {
        if (Table == "COPYLROW") {
            if (event.key != "F8") {
                return true;
            }
            else {
                i = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length - 1;
            }
        }
        if (str != "") {
            $("#TXNSLNO").val(returncolvalue(str, "TXNSLNO"));
            $("#ITGRPCD").val(returncolvalue(str, "ITGRPCD"));
            $("#ITGRPNM").val(returncolvalue(str, "ITGRPNM"));
            $("#MTRLJOBCD").val(returncolvalue(str, "MTRLJOBCD"));
            $("#MTRLJOBNM").val(returncolvalue(str, "MTRLJOBNM"));
            $("#MTBARCODE").val(returncolvalue(str, "MTBARCODE"));
            $("#ITCD").val(returncolvalue(str, "ITCD"));
            $("#ITSTYLE").val(returncolvalue(str, "STYLENO") + "" + returncolvalue(str, "ITNM"));
            $("#STYLENO").val(returncolvalue(str, "STYLENO"));
            $("#STKTYPE").val(returncolvalue(str, "STKTYPE"));
            $("#PARTCD").val(returncolvalue(str, "PARTCD"));
            $("#PARTNM").val(returncolvalue(str, "PARTNM"));
            $("#PRTBARCODE").val(returncolvalue(str, "PRTBARCODE"));
            $("#COLRCD").val(returncolvalue(str, "COLRCD"));
            $("#COLRNM").val(returncolvalue(str, "COLRNM"));
            $("#CLRBARCODE").val(returncolvalue(str, "CLRBARCODE"));
            $("#SIZECD").val(returncolvalue(str, "SIZECD"));
            $("#SIZENM").val(returncolvalue(str, "SIZENM"));
            $("#SZBARCODE").val(returncolvalue(str, "SZBARCODE"));
            $("#PDESIGN").val(returncolvalue(str, "PDESIGN"));
            $("#BALSTOCK").val(returncolvalue(str, "BALQNTY"));
            $("#QNTY").val(returncolvalue(str, "QNTY"));
            $("#UOM").val(returncolvalue(str, "uomcd"));
            $("#NOS").val(returncolvalue(str, "NOS"));
            $("#FLAGMTR").val(returncolvalue(str, "FLAGMTR"));
            $("#RATE").val(returncolvalue(str, "RATE"));
            $("#DISCRATE").val(returncolvalue(str, "DISCRATE"));
            $("#DISCTYPE").val(returncolvalue(str, "DISCTYPE"));
            $("#HSNCODE").val(returncolvalue(str, "HSNCODE"));
            $("#GSTPER").val(returncolvalue(str, "GSTPER"));
            $("#ALL_GSTPER").val(returncolvalue(str, "ALL_GSTPER"));
            $("#PRODGRPGSTPER").val(returncolvalue(str, "PRODGRPGSTPER"));
            $("#SHADE").val(returncolvalue(str, "SHADE"));
            if ("@Model.MENU_PARA" == "PB") {
                $("#BALENO").val(returncolvalue(str, "BALENO"));
                $("#OURDESIGN").val(returncolvalue(str, "OURDESIGN"));
            }
            $("#TDDISCTYPE").val(returncolvalue(str, "TDDISCTYPE"));
            $("#TDDISCRATE").val(returncolvalue(str, "TDDISCRATE"));
            $("#SCMDISCTYPE").val(returncolvalue(str, "SCMDISCTYPE"));
            $("#SCMDISCRATE").val(returncolvalue(str, "SCMDISCRATE"));
            $("#LOCABIN").val(returncolvalue(str, "LOCABIN"));
            $("#GLCD").val(returncolvalue(str, "GLCD"));
        }
        else {
            var FieldidStarting = "";
            if (Table == "_T_SALE_BARNODETAIL_GRID") {
                var GridRowMain = $("#_T_SALE_BARNODETAIL_GRID > tbody > tr").length;
                var cnt = 0;
                for (j = 0; j <= GridRowMain - 1; j++) {
                    if (document.getElementById("STOCK_Checked_" + j).checked == true) {
                        i = j;
                        cnt++;
                        break;
                    }
                }
                if (cnt == 0) {
                    msgInfo("Please Select row from stock details !");
                    return false;
                }
                else {
                    FieldidStarting = "#STOCK_";
                }
            }
            else {
                FieldidStarting = "#B_";
            }
            if (Table != "_T_SALE_BARNODETAIL_GRID") {
                $("#SLNO").val($(FieldidStarting + "SLNO_" + i).val());
                $("#TXNSLNO").val($(FieldidStarting + "TXNSLNO_" + i).val());
                $("#STKNAME").val($(FieldidStarting + "STKNAME_" + i).val());
                if (Table != "COPYLROW") {
                    $("#QNTY").val($(FieldidStarting + "QNTY_" + i).val());
                    $("#NOS").val($(FieldidStarting + "NOS_" + i).val());
                }
                $("#DISCRATE").val($(FieldidStarting + "DISCRATE_" + i).val());
                $("#DISCTYPE").val($(FieldidStarting + "DISCTYPE_" + i).val());
                $("#TDDISCTYPE").val($(FieldidStarting + "TDDISCTYPE_" + i).val());
                $("#TDDISCRATE").val($(FieldidStarting + "TDDISCRATE_" + i).val());
                $("#SCMDISCTYPE").val($(FieldidStarting + "SCMDISCTYPE_" + i).val());
                $("#SCMDISCRATE").val($(FieldidStarting + "SCMDISCRATE_" + i).val());

            }
            $("#BARCODE").val($(FieldidStarting + "BARNO_" + i).val());
            $("#ITGRPCD").val($(FieldidStarting + "ITGRPCD_" + i).val());
            $("#ITGRPNM").val($(FieldidStarting + "ITGRPNM_" + i).val());
            $("#MTRLJOBCD").val($(FieldidStarting + "MTRLJOBCD_" + i).val());
            $("#MTRLJOBNM").val($(FieldidStarting + "MTRLJOBNM_" + i).val());
            $("#MTBARCODE").val($(FieldidStarting + "MTBARCODE_" + i).val());
            $("#ITCD").val($(FieldidStarting + "ITCD_" + i).val());
            $("#ITSTYLE").val($(FieldidStarting + "ITSTYLE_" + i).val());
            $("#STYLENO").val($(FieldidStarting + "STYLENO_" + i).val());
            $("#STKTYPE").val($(FieldidStarting + "STKTYPE_" + i).val());
            $("#PARTCD").val($(FieldidStarting + "PARTCD_" + i).val());
            $("#PRTBARCODE").val($(FieldidStarting + "PRTBARCODE_" + i).val());
            $("#COLRCD").val($(FieldidStarting + "COLRCD_" + i).val());
            $("#COLRNM").val($(FieldidStarting + "COLRNM_" + i).val());
            $("#CLRBARCODE").val($(FieldidStarting + "CLRBARCODE_" + i).val());
            $("#SIZECD").val($(FieldidStarting + "SIZECD_" + i).val());
            $("#SIZENM").val($(FieldidStarting + "SIZENM_" + i).val());
            $("#SZBARCODE").val($(FieldidStarting + "SZBARCODE_" + i).val());
            $("#PDESIGN").val($(FieldidStarting + "PDESIGN_" + i).val());
            $("#FLAGMTR").val($(FieldidStarting + "FLAGMTR_" + i).val());
            $("#RATE").val($(FieldidStarting + "RATE_" + i).val());
            $("#HSNCODE").val($(FieldidStarting + "HSNCODE_" + i).val());
            $("#GSTPER").val($(FieldidStarting + "GSTPER_" + i).val());
            $("#ALL_GSTPER").val($(FieldidStarting + "ALL_GSTPER_" + i).val());
            $("#PRODGRPGSTPER").val($(FieldidStarting + "PRODGRPGSTPER_" + i).val());
            $("#SHADE").val($(FieldidStarting + "SHADE_" + i).val());
            if ("@Model.MENU_PARA" == "PB") {
                $("#BALENO").val($(FieldidStarting + "BALENO_" + i).val());
                $("#OURDESIGN").val($(FieldidStarting + "OURDESIGN_" + i).val());
            }
            $("#LOCABIN").val($(FieldidStarting + "LOCABIN_" + i).val());
            if (Table == "_T_SALE_BARNODETAIL_GRID") {
                $("#BALSTOCK").val($(FieldidStarting + "BALQNTY_" + i).val());
            }
            $("#UOM").val($(FieldidStarting + "UOM_" + i).val());
            $("#GLCD").val($(FieldidStarting + "GLCD_" + i).val());
            if (Table == "_T_SALE_BARNODETAIL_GRID") {
                CloseBarnoPopup();
            }
        }
        if (Table == "_T_SALE_PRODUCT_GRID") {
            $("#AddRow_Barcode").hide();
            $("#UpdateRow_Barcode").prop("value", "Update Row [" + $(FieldidStarting + "SLNO_" + i).val() + "] (Alt+W)");
            $("#UpdateRow_Barcode").show();

        }
    }
}
function CloseBarnoPopup() {
    if ("@Model.DefaultAction" != "V") {
        $("#popup").html("");
    }
}
function UpdateBarCodeRow() {
    if ("@Model.DefaultAction" != "V") {
        debugger;
        if ($("#ITGRPCD").val() == "") {
            msgInfo("Please enter/select Item Group Code !");
            message_value = "ITGRPCD";
            return false;
        }
        if ($("#MTRLJOBCD").val() == "") {
            msgInfo("Please enter Material Job Code !");
            message_value = "MTRLJOBCD";
            return false;
        }
        if ($("#ITCD").val() == "") {
            msgInfo("Please enter Item Code !");
            message_value = "ITCD";
            return false;
        }
        if ($("#QNTY").val() == "0") {
            msgInfo("Quantity should not be zero(0) !");
            message_value = "QNTY";
            return false;
        }
        var TXNSLNO = "";
        if ($("#TXNSLNO").val() == "") {
            var GridRowMain = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
            if (GridRowMain == 0) {
                TXNSLNO = 1;
            }
            else {
                var allslno = [parseInt(GridRowMain)];
                for (j = 0; j <= GridRowMain - 1; j++) {
                    allslno[j] = parseInt($("#B_TXNSLNO_" + j).val());
                }
                TXNSLNO = Math.max.apply(Math, allslno);
                TXNSLNO++;
            }
        }
        else {
            TXNSLNO = parseInt($("#TXNSLNO").val());
        }
        var FLAGMTR = $("#FLAGMTR").val();
        var QNTY = $("#QNTY").val();

        if (FLAGMTR != "") {
            var flgmtr = FLAGMTR;
            if (flgmtr == "") { flgmtr = parseFloat(0); } else { flgmtr = parseFloat(flgmtr); }
            var qnty = QNTY;
            if (qnty == "") { qnty = parseFloat(0); } else { qnty = parseFloat(qnty); }
            if (flgmtr > qnty) {
                msgInfo("FLAGMTR (" + flgmtr + ") should be less than Quantity (" + qnty + ") !");
                message_value = "FLAGMTR";
                return false;
            }
        }
        if ($("#HSNCODE").val() == "") {
            msgInfo("Please enter HSN Code !");
            message_value = "HSNCODE";
            return false;
        }
        var SLNO = $("#SLNO").val();
        var GridRowMain = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
        for (j = 0; j <= GridRowMain - 1; j++) {
            if ($("#B_SLNO_" + j).val() == SLNO) {
                $("#B_BARNO_" + j).val($("#BARCODE").val());
                $("#B_TXNSLNO_" + j).val(TXNSLNO);
                $("#B_ITGRPCD_" + j).val($("#ITGRPCD").val());
                $("#B_ITGRPNM_" + j).val($("#ITGRPNM").val());
                $("#B_MTRLJOBCD_" + j).val($("#MTRLJOBCD").val());
                $("#B_MTRLJOBNM_" + j).val($("#MTRLJOBNM").val());
                $("#B_MTBARCODE_" + j).val($("#MTBARCODE").val());
                $("#B_ITCD_" + j).val($("#ITCD").val());
                $("#B_ITSTYLE_" + j).val($("#ITSTYLE").val());
                $("#B_STYLENO_" + j).val($("#STYLENO").val());
                $("#B_STKTYPE_" + j).val($("#STKTYPE").val());
                $("#B_PARTCD_" + j).val($("#PARTCD").val());
                $("#B_PARTNM_" + j).val($("#PARTNM").val());
                $("#B_PRTBARCODE_" + j).val($("#PRTBARCODE").val());
                $("#B_COLRCD_" + j).val($("#COLRCD").val());
                $("#B_COLRNM_" + j).val($("#COLRNM").val());
                $("#B_CLRBARCODE_" + j).val($("#CLRBARCODE").val());
                $("#B_SIZECD_" + j).val($("#SIZECD").val());
                $("#B_SIZENM_" + j).val($("#SIZENM").val());
                $("#B_SZBARCODE_" + j).val($("#SZBARCODE").val());
                $("#B_PDESIGN_" + j).val($("#PDESIGN").val());
                $("#B_BALSTOCK_" + j).val($("#BALSTOCK").val());
                $("#B_QNTY_" + j).val($("#QNTY").val());
                $("#B_UOM_" + j).val($("#UOM").val());
                $("#B_NOS_" + j).val($("#NOS").val());
                $("#B_FLAGMTR_" + j).val($("#FLAGMTR").val());
                $("#B_RATE_" + j).val($("#RATE").val());
                $("#B_DISCRATE_" + j).val($("#DISCRATE").val());
                $("#B_DISCTYPE_" + j).val($("#DISCTYPE").val());
                $("#B_HSNCODE_" + j).val($("#HSNCODE").val());
                $("#B_GSTPER_" + j).val($("#GSTPER").val());
                $("#B_ALL_GSTPER_" + j).val($("#ALL_GSTPER").val());
                $("#B_PRODGRPGSTPER_" + j).val($("#PRODGRPGSTPER").val());
                $("#B_SHADE_" + j).val($("#SHADE").val());
                if ("@Model.MENU_PARA" == "PB") {
                    $("#B_BALENO_" + j).val($("#BALENO").val());
                    $("#B_OURDESIGN_" + j).val($("#OURDESIGN").val());
                }
                $("#B_TDDISCTYPE_" + j).val($("#TDDISCTYPE").val());
                $("#B_TDDISCRATE_" + j).val($("#TDDISCRATE").val());
                $("#B_SCMDISCTYPE_" + j).val($("#SCMDISCTYPE").val());
                $("#B_SCMDISCRATE_" + j).val($("#SCMDISCRATE").val());
                $("#B_LOCABIN_" + j).val($("#LOCABIN").val());
                var DISCTYPE = $("#DISCTYPE").val() == "P" ? "%" : $("#DISCTYPE").val() == "N" ? "Nos" : $("#DISCTYPE").val() == "Q" ? "Qnty" : "Fixed";
                var TDDISCTYPE = $("#TDDISCTYPE").val() == "P" ? "%" : $("#TDDISCTYPE").val() == "N" ? "Nos" : $("#TDDISCTYPE").val() == "Q" ? "Qnty" : "Fixed";
                var SCMDISCTYPE = $("#SCMDISCTYPE").val() == "P" ? "%" : $("#SCMDISCTYPE").val() == "N" ? "Nos" : $("#SCMDISCTYPE").val() == "Q" ? "Qnty" : "Fixed";
                $("#B_DISCTYPE_DESC_" + j).val(DISCTYPE);
                $("#B_TDDISCTYPE_DESC_" + j).val(TDDISCTYPE);
                $("#B_SCMDISCTYPE_DESC_" + j).val(SCMDISCTYPE);
                $("#B_GLCD_" + j).val($("#GLCD").val());

            }
        }
        CalculateTotal_Barno();
        ClearBarcodeArea();
        @*if ("@Model.MENU_PARA" == "PB") {
            $("#BALENO").val("");
        }
        $("#DISCTYPE").val("P");
        $("#TDDISCTYPE").val("P");
        $("#SCMDISCTYPE").val("P");*@
            $("#AddRow_Barcode").show();
        $("#UpdateRow_Barcode").hide();
        if ("@Model.MENU_PARA" == "PB") {
            $("#TXNSLNO").focus();
        } else {
            $("#BARCODE").focus();
        }
    }
}

function ClearBarcodeArea(TAG) {
    if ("@Model.DefaultAction" != "V") {
        ClearAllTextBoxes("BARCODE,TXNSLNO,ITGRPCD,ITGRPNM,MTRLJOBCD,MTRLJOBNM,MTBARCODE,ITCD,ITSTYLE,STYLENO,STKTYPE,PARTCD,PARTNM,PRTBARCODE,COLRCD,COLRNM,CLRBARCODE,SIZECD,SIZENM,SZBARCODE,PDESIGN,BALSTOCK,QNTY,UOM,GLCD,NOS,FLAGMTR,RATE,DISCRATE,HSNCODE,GSTPER,ALL_GSTPER,PRODGRPGSTPER,SHADE,TDDISCRATE,SCMDISCRATE,LOCABIN");
        if ("@Model.MENU_PARA" == "PB") {
            $("#BALENO").val("");
            $("#OURDESIGN").val("");
        }
        $("#DISCTYPE").val("P");
        $("#TDDISCTYPE").val("P");
        $("#SCMDISCTYPE").val("P");
        if (TAG == "Y") {
            $("#AddRow_Barcode").show();
            $("#UpdateRow_Barcode").hide();
            if ("@Model.MENU_PARA" == "PB") {
                $("#TXNSLNO").focus();
            } else {
                $("#BARCODE").focus();
            }
        }
    }
}
</script>

<script>
    function Fill_DetailData() {
        if ("@Model.DefaultAction" != "V") {
            $.ajax({
                type: 'POST',
                beforesend: $("#WaitingMode").show(),
                url: "@Url.Action("FillDetailData", PageControllerName)",
                data: $('form').serialize(),
            success: function (result) {
                //$("#partialdivBarCodeTab").animate({ marginTop: '-10px' }, 50);
                $("#partialdivDetail").html(result);
                $("li").removeClass("active").addClass("");
                $(".nav-tabs li:nth-child(3)").addClass('active');
                //below set the  child sequence
                $(".tab-content div").removeClass("active");
                $(".tab-content div:nth-child(3)").removeClass("tab-pane fade").addClass("tab-pane fade in active");
                var GridRow = $("#_T_SALE_DETAIL_GRID > tbody > tr").length;
                for (var i = 0; i <= GridRow - 1; i++) {
                    CalculateAmt_Details(i);
                }
                $("#WaitingMode").hide();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $("#WaitingMode").hide();
                msgError(XMLHttpRequest.responseText);
                $("body span h1").remove(); $("#msgbody_error style").remove();
            }
        });
    }
}
function UpdateBarCodeRow_FrmDet(i) {
    if ("@Model.DefaultAction" != "V") {
        var TXNSLNO = $("#D_SLNO_" + i).val();
        var ITGRPCD = $("#D_ITGRPCD_" + i).val();
        var MTRLJOBCD = $("#D_MTRLJOBCD_" + i).val();
        var ITCD = $("#D_ITCD_" + i).val();
        var STKTYPE = $("#D_STKTYPE_" + i).val();
        //fill
        var RATE = $("#D_RATE_" + i).val();
        var DISCTYPE = $("#D_DISCTYPE_" + i).val();
        var DISCTYPE_DESC = $("#D_DISCTYPE_DESC_" + i).val();
        var DISCRATE = $("#D_DISCRATE_" + i).val();
        var TDDISCRATE = $("#D_TDDISCRATE_" + i).val();
        var TDDISCTYPE = $("#D_TDDISCTYPE_" + i).val();
        var SCMDISCRATE = $("#D_SCMDISCRATE_" + i).val();
        var SCMDISCTYPE = $("#D_SCMDISCTYPE_" + i).val();
        var BALENO = $("#D_BALENO_" + i).val();
        var ALLGSTPER = $("#D_ALL_GSTPER_" + i).val();
        var GridRowMain = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
        for (j = 0; j <= GridRowMain - 1; j++) {
            if ($("#B_TXNSLNO_" + j).val() == TXNSLNO && $("#B_ITGRPCD_" + j).val() == ITGRPCD && $("#B_MTRLJOBCD_" + j).val() == MTRLJOBCD && $("#B_ITCD_" + j).val() == ITCD && $("#B_STKTYPE_" + j).val() == STKTYPE) {
                $("#B_RATE_" + j).val(RATE);
                $("#B_DISCTYPE_" + j).val(DISCTYPE);
                $("#B_DISCTYPE_DESC_" + j).val(DISCTYPE_DESC);
                $("#B_DISCRATE_" + j).val(DISCRATE);
                $("#B_TDDISCRATE_" + j).val(TDDISCRATE);
                $("#B_TDDISCTYPE_" + j).val(TDDISCTYPE);
                $("#B_SCMDISCRATE_" + j).val(SCMDISCRATE);
                $("#B_SCMDISCTYPE_" + j).val(SCMDISCTYPE);
                $("#B_BALENO_" + j).val(BALENO);
                $("#B_ALL_GSTPER_" + j).val(ALLGSTPER);
                if (ALLGSTPER != "") {
                    var str = ALLGSTPER.split(',');
                    if (str.length > 0) {
                        var sumgst = parseFloat(parseFloat(str[0]) + parseFloat(str[1]) + parseFloat(str[2])).toFixed(2);
                        $("#B_GSTPER_" + j).val(sumgst);
                    }
                }
            }
        }
    }
}
</script>
<script>
    function CalculateTotal_Barno() {
        if ("@Model.DefaultAction" != "V") {
            var T_QNTY = 0, T_NOS = 0;
            var GridRow = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
            for (var i = 0; i <= GridRow - 1; i++) {
                var QNTY = $("#B_QNTY_" + i).val();
                if (QNTY != "") { T_QNTY = T_QNTY + parseFloat(QNTY); } else { T_QNTY = T_QNTY + parseFloat(0); }
                var NOS = $("#B_NOS_" + i).val();
                if (NOS != "") { T_NOS = T_NOS + parseFloat(NOS); } else { T_NOS = T_NOS + parseFloat(0); }
            }
            $("#B_T_QNTY").val(parseFloat(T_QNTY).toFixed(2));
            $("#B_T_NOS").val(parseFloat(T_NOS).toFixed(0));
        }
    }
function CalculateAmt_Details(i) {
    if ("@Model.DefaultAction" != "V") {
        var BLQNTY = $("#D_BLQNTY_" + i).val();
        if (BLQNTY != "") { BLQNTY = parseFloat(BLQNTY); } else { BLQNTY = parseFloat(0); }

        var QNTY = $("#D_QNTY_" + i).val();
        if (QNTY != "") { QNTY = parseFloat(QNTY); } else { QNTY = parseFloat(0); }

        var NOS = $("#D_NOS_" + i).val();
        if (NOS != "") { NOS = parseFloat(NOS); } else { NOS = parseFloat(0); }

        var FLAGMTR = $("#D_FLAGMTR_" + i).val();
        if (FLAGMTR != "") { FLAGMTR = parseFloat(FLAGMTR); } else { FLAGMTR = parseFloat(0); }

        var RATE = $("#D_RATE_" + i).val();
        if (RATE != "") { RATE = parseFloat(RATE); } else { RATE = parseFloat(0); }

        var IGSTPER = $("#D_IGSTPER_" + i).val();
        if (IGSTPER != "") { IGSTPER = parseFloat(IGSTPER); } else { IGSTPER = parseFloat(0); }

        var CGSTPER = $("#D_CGSTPER_" + i).val();
        if (CGSTPER != "") { CGSTPER = parseFloat(CGSTPER); } else { CGSTPER = parseFloat(0); }

        var SGSTPER = $("#D_SGSTPER_" + i).val();
        if (SGSTPER != "") { SGSTPER = parseFloat(SGSTPER); } else { SGSTPER = parseFloat(0); }

        var CESSPER = $("#D_CESSPER_" + i).val();
        if (CESSPER != "") { CESSPER = parseFloat(CESSPER); } else { CESSPER = parseFloat(0); }

        var DISCTYPE = $("#D_DISCTYPE_" + i).val();
        var DISCRATE = $("#D_DISCRATE_" + i).val();
        if (DISCRATE != "") { DISCRATE = parseFloat(DISCRATE); } else { DISCRATE = parseFloat(0); }

        var TDDISCTYPE = $("#D_TDDISCTYPE_" + i).val();
        var TDDISCRATE = $("#D_TDDISCRATE_" + i).val();
        if (TDDISCRATE != "") { TDDISCRATE = parseFloat(TDDISCRATE); } else { TDDISCRATE = parseFloat(0); }

        var SCMDISCTYPE = $("#D_SCMDISCTYPE_" + i).val();
        var SCMDISCRATE = $("#D_SCMDISCRATE_" + i).val();
        if (SCMDISCRATE != "") { SCMDISCRATE = parseFloat(SCMDISCRATE); } else { SCMDISCRATE = parseFloat(0); }


        //AMOUNT CALCULATION
        var amount = 0;
        if (BLQNTY == 0) {
            amount = (parseFloat(QNTY) - parseFloat(FLAGMTR)) * parseFloat(RATE);
        }
        else {
            amount = parseFloat(BLQNTY) * parseFloat(RATE);
        }
        amount = parseFloat(amount).toFixed(2);
        $("#D_AMT_" + i).val(amount);

        //DISCOUNT AMOUNT CALCULATION
        var DISCAMT = 0;
        if (DISCTYPE == "Q") { DISCAMT = DISCRATE * QNTY; }
        else if (DISCTYPE == "N") { DISCAMT = DISCRATE * NOS; }
        else if (DISCTYPE == "P") { DISCAMT = (amount * DISCRATE) / 100; }
        else if (DISCTYPE == "F") { DISCAMT = DISCRATE; }
        else { DISCAMT = 0; }
        DISCAMT = parseFloat(DISCAMT).toFixed(2);
        $("#D_DISCAMT_" + i).val(DISCAMT);

        //TDDISCOUNT AMOUNT CALCULATION
        var TDDISCAMT = 0;
        if (TDDISCTYPE == "Q") { TDDISCAMT = TDDISCRATE * QNTY; }
        else if (TDDISCTYPE == "N") { TDDISCAMT = TDDISCRATE * NOS; }
        else if (TDDISCTYPE == "P") { TDDISCAMT = (amount * TDDISCRATE) / 100; }
        else if (TDDISCTYPE == "F") { TDDISCAMT = TDDISCRATE; }
        else { TDDISCAMT = 0; }
        TDDISCAMT = parseFloat(TDDISCAMT).toFixed(2);
        $("#D_TDDISCAMT_" + i).val(TDDISCAMT);

        //SCMDISCOUNT AMOUNT CALCULATION
        var SCMDISCAMT = 0;
        if (SCMDISCTYPE == "Q") { SCMDISCAMT = SCMDISCRATE * QNTY; }
        else if (SCMDISCTYPE == "N") { SCMDISCAMT = SCMDISCRATE * NOS; }
        else if (SCMDISCTYPE == "P") { SCMDISCAMT = (amount * SCMDISCRATE) / 100; }
        else if (SCMDISCTYPE == "F") { SCMDISCAMT = SCMDISCRATE; }
        else { SCMDISCAMT = 0; }
        SCMDISCAMT = parseFloat(SCMDISCAMT).toFixed(2);
        $("#D_SCMDISCAMT_" + i).val(SCMDISCAMT);

        //TOTAL DISCOUNT AMOUNT CALCULATION
        var TOTDISCAMT = parseFloat(DISCAMT + TDDISCAMT + SCMDISCAMT).toFixed(2);
        $("#D_TOTDISCAMT_" + i).val(TOTDISCAMT);

        //TAXABLE CALCULATION
        var taxbleamt = parseFloat(amount) - parseFloat(TOTDISCAMT);
        taxbleamt = parseFloat(taxbleamt).toFixed(2);
        $("#D_TXBLVAL_" + i).val(taxbleamt);
        //IGST,CGST,SGST,CESS AMOUNT CALCULATION

        var IGST_AMT = 0; var CGST_AMT = 0; var SGST_AMT = 0; var CESS_AMT = 0; var chkAmt = 0;

        //IGST
        if (IGSTPER == 0 || IGSTPER == "") {
            IGSTPER = 0; IGST_AMT = 0;
        }
        else {
            IGST_AMT = parseFloat((taxbleamt * IGSTPER) / 100).toFixed(2);
            chkAmt = $("#D_IGSTAMT_" + i).val();
            if (chkAmt == "") chkAmt = 0;
            if (Math.abs(IGST_AMT - chkAmt) <= 1) IGST_AMT = chkAmt;
        }
        $("#D_IGSTAMT_" + i).val(IGST_AMT);
        //CGST
        if (CGSTPER == "" || CGSTPER == 0) {
            CGSTPER = 0; CGST_AMT = 0;
        }
        else {
            CGST_AMT = parseFloat((taxbleamt * CGSTPER) / 100).toFixed(2);
            chkAmt = $("#D_CGSTAMT_" + i).val();
            if (chkAmt == "") chkAmt = 0;
            if (Math.abs(CGST_AMT - chkAmt) <= 1) CGST_AMT = chkAmt;
        }
        $("#D_CGSTAMT_" + i).val(CGST_AMT);
        //SGST
        if (SGSTPER == "" || SGSTPER == 0) {
            SGSTPER = 0; SGST_AMT = 0;
        }
        else {
            SGST_AMT = parseFloat((taxbleamt * SGSTPER) / 100).toFixed(2);
            chkAmt = $("#D_SGSTAMT_" + i).val();
            if (chkAmt == "") chkAmt = 0;
            if (Math.abs(SGST_AMT - chkAmt) <= 1) SGST_AMT = chkAmt;
        }
        $("#D_SGSTAMT_" + i).val(SGST_AMT);

        //CESS
        if (CESSPER == "" || CESSPER == 0) {
            CESSPER = 0; CESS_AMT = 0;
        }
        else {
            CESS_AMT = parseFloat((taxbleamt * CESSPER) / 100).toFixed(2);
            chkAmt = $("#D_CESSAMT_" + i).val();
            if (chkAmt == "") chkAmt = 0;
            if (Math.abs(CESS_AMT - chkAmt) <= 1) CESS_AMT = chkAmt;
        }
        $("#D_CESSAMT_" + i).val(CESS_AMT);

        var netamt = parseFloat(parseFloat(taxbleamt) + parseFloat(IGST_AMT) + parseFloat(CGST_AMT) + parseFloat(SGST_AMT) + parseFloat(CESS_AMT)).toFixed(2);
        $("#D_NETAMT_" + i).val(netamt);
        CalculateTotal_Details();
    }
}
function CalculateTotal_Details() {
    if ("@Model.DefaultAction" != "V") {
        var T_NOS = 0; var T_QNTY = 0; var T_AMT = 0; var T_GROSS_AMT = 0; var T_IGST_AMT = 0; var T_CGST_AMT = 0; var T_SGST_AMT = 0; var T_CESS_AMT = 0; var T_NET_AMT = 0;

        var GridRow = $("#_T_SALE_DETAIL_GRID > tbody > tr").length;
        for (var i = 0; i <= GridRow - 1; i++) {
            var NOS = $("#D_NOS_" + i).val();
            if (NOS != "") { T_NOS = T_NOS + parseFloat(NOS); } else { T_NOS = T_NOS + parseFloat(0); }

            var QNTY = $("#D_QNTY_" + i).val();
            if (QNTY != "") { T_QNTY = T_QNTY + parseFloat(QNTY); } else { T_QNTY = T_QNTY + parseFloat(0); }

            var AMT = $("#D_AMT_" + i).val();
            if (AMT != "") { T_AMT = T_AMT + parseFloat(AMT); } else { T_AMT = T_AMT + parseFloat(0); }

            var GROSS_AMT = $("#D_TXBLVAL_" + i).val();
            if (GROSS_AMT != "") { T_GROSS_AMT = T_GROSS_AMT + parseFloat(GROSS_AMT); } else { T_GROSS_AMT = T_GROSS_AMT + parseFloat(0); }

            var IGST_AMT = $("#D_IGSTAMT_" + i).val();
            if (IGST_AMT != "") { T_IGST_AMT = T_IGST_AMT + parseFloat(IGST_AMT); } else { T_IGST_AMT = T_IGST_AMT + parseFloat(0); }

            var CGST_AMT = $("#D_CGSTAMT_" + i).val();
            if (CGST_AMT != "") { T_CGST_AMT = T_CGST_AMT + parseFloat(CGST_AMT); } else { T_CGST_AMT = T_CGST_AMT + parseFloat(0); }

            var SGST_AMT = $("#D_SGSTAMT_" + i).val();
            if (SGST_AMT != "") { T_SGST_AMT = T_SGST_AMT + parseFloat(SGST_AMT); } else { T_SGST_AMT = T_SGST_AMT + parseFloat(0); }

            var CESS_AMT = $("#D_CESSAMT_" + i).val();
            if (CESS_AMT != "") { T_CESS_AMT = T_CESS_AMT + parseFloat(CESS_AMT); } else { T_CESS_AMT = T_CESS_AMT + parseFloat(0); }

            var NET_AMT = $("#D_NETAMT_" + i).val();
            if (NET_AMT != "") { T_NET_AMT = T_NET_AMT + parseFloat(NET_AMT); } else { T_NET_AMT = T_NET_AMT + parseFloat(0); }
        }
        var totaltax = parseFloat(T_IGST_AMT) + parseFloat(T_CGST_AMT) + parseFloat(T_SGST_AMT) + parseFloat(T_CESS_AMT);
        $("#T_NOS").val(parseFloat(T_NOS).toFixed(0));
        $("#T_QNTY").val(parseFloat(T_QNTY).toFixed(2));
        $("#T_AMT").val(parseFloat(T_AMT).toFixed(2));
        $("#T_GROSS_AMT").val(parseFloat(T_GROSS_AMT).toFixed(2));
        $("#T_IGST_AMT").val(parseFloat(T_IGST_AMT).toFixed(2));
        $("#T_CGST_AMT").val(parseFloat(T_CGST_AMT).toFixed(2));
        $("#T_SGST_AMT").val(parseFloat(T_SGST_AMT).toFixed(2));
        $("#T_CESS_AMT").val(parseFloat(T_CESS_AMT).toFixed(2));
        $("#T_NET_AMT").val(parseFloat(T_NET_AMT).toFixed(2));

        //main tab
        //$("#TOTNOS").val(parseFloat(T_NOS).toFixed(2));
        //$("#TOTQNTY").val(parseFloat(T_QNTY).toFixed(2));
        //$("#TOTTAXVAL").val(parseFloat(T_GROSS_AMT).toFixed(2));
        //$("#TOTTAX").val(parseFloat(totaltax).toFixed(2));
        BillAmountCalculate();
    }
}
</script>
<script>
    function GetGstPer(i, FieldidStarting) {
        if ("@Model.DefaultAction" != "V") {
            debugger;
            var prodgrpgstper = "";
            var rate = 0;
            if (FieldidStarting == "") {
                prodgrpgstper = $("#PRODGRPGSTPER").val();
                rate = $("#RATE").val();
                if (rate == "") { rate = parseFloat(0); } else { rate = parseFloat(rate); }
            }
            else {
                prodgrpgstper = $(FieldidStarting + "PRODGRPGSTPER_" + i).val();
                rate = $(FieldidStarting + "RATE_" + i).val();
                if (rate == "") { rate = parseFloat(0); } else { rate = parseFloat(rate); }
            }
            var allgst = retGstPer(prodgrpgstper, rate);
            if (allgst != "") {
                var str = allgst.split(',');
                if (str.length > 0) {
                    var sumgst = parseFloat(parseFloat(str[0]) + parseFloat(str[1]) + parseFloat(str[2])).toFixed(2);
                    if (FieldidStarting == "") {
                        $("#GSTPER").val(sumgst);
                        $("#ALL_GSTPER").val(allgst);
                    }
                    else if (FieldidStarting == "#B_") {

                        $(FieldidStarting + "GSTPER_" + i).val(sumgst);
                        $(FieldidStarting + "ALL_GSTPER_" + i).val(allgst);
                    }
                    else {
                        $(FieldidStarting + "IGSTPER_" + i).val(parseFloat(str[0]).toFixed(2));
                        $(FieldidStarting + "CGSTPER_" + i).val(parseFloat(str[1]).toFixed(2));
                        $(FieldidStarting + "SGSTPER_" + i).val(parseFloat(str[2]).toFixed(2));
                        $(FieldidStarting + "ALL_GSTPER_" + i).val(allgst);
                    }
                }
            }
            if (FieldidStarting == "#D_") {
                UpdateBarCodeRow_FrmDet(i);
                CalculateAmt_Details(i);
            }
        }
    }
function retGstPer(prodgrpgstper, rate) {
    if ("@Model.DefaultAction" != "V") {
        //Searchstr value like listagg(b.fromrt||chr(126)||b.tort||chr(126)||b.igstper||chr(126)||b.cgstper||chr(126)||b.sgstper,chr(179))

        var fromrt = 0, tort = 0, selrow = -1;
        var mgstrate = [5];
        var rtval = "0,0,0"; //igstper,cgst,sgst
        var SP = String.fromCharCode(179);

        var mrates = prodgrpgstper.split(SP);
        for (var x = 0; x <= mrates.length - 1; x++) {
            //mgstrate = mrates[x].Split(Convert.ToChar(Cn.GCS())).ToArray();
            mgstrate = mrates[x].split('~');

            if (mgstrate[0] == "") { fromrt = parseFloat(0); } else { fromrt = parseFloat(mgstrate[0]); }
            if (mgstrate[1] == "") { tort = parseFloat(0); } else { tort = parseFloat(mgstrate[1]); }
            if (rate >= fromrt && rate <= tort) { selrow = x; break; }
        }
        if (selrow != -1) rtval = mgstrate[2] + "," + mgstrate[3] + "," + mgstrate[4];
        return rtval;
    }
}
function ItcdClr() {
    if ("@Model.DefaultAction" != "V") {
        ClearAllTextBoxes("ITCD,ITSTYLE,UOM,STYLENO,HSNCODE");
    }
}
</script>
<script>
    function AmountCalculation(i) {
        if ("@Model.DefaultAction" != "V") {
            var A_NOS = document.getElementById("T_NOS").value;
            var B_QNTY = document.getElementById("T_QNTY").value;
            var D_GROSS_AMT = document.getElementById("T_GROSS_AMT").value;
            var E_NET_AMT = document.getElementById("T_NET_AMT").value;
            var RT = document.getElementById("AMTRATE_" + i).value;
            var IGST_PER = document.getElementById("AIGSTPER_" + i).value;
            if (IGST_PER == "") { IGST_PER = parseFloat(0); } else { IGST_PER = parseFloat(IGST_PER) }
            var CGST_PER = document.getElementById("ACGSTPER_" + i).value;
            if (CGST_PER == "") { CGST_PER = parseFloat(0); } else { CGST_PER = parseFloat(CGST_PER) }
            var SGST_PER = document.getElementById("ASGSTPER_" + i).value;
            if (SGST_PER == "") { SGST_PER = parseFloat(0); } else { SGST_PER = parseFloat(SGST_PER) }
            var CESS_PER = document.getElementById("ACESSPER_" + i).value;
            if (CESS_PER == "") { CESS_PER = parseFloat(0); } else { CESS_PER = parseFloat(CESS_PER) }
            var DUTY_PER = document.getElementById("ADUTYPER_" + i).value;
            if (DUTY_PER == "") { DUTY_PER = parseFloat(0); } else { DUTY_PER = parseFloat(DUTY_PER) }
            var CALC_TYPE = document.getElementById("CALCTYPE_" + i).value;
            var CALC_FORMULA = document.getElementById("CALCFORMULA_" + i).value;
            if (A_NOS == "") { A_NOS = parseFloat(0); }
            if (B_QNTY == "") { B_QNTY = parseFloat(0); }
            if (D_GROSS_AMT == "") { D_GROSS_AMT = parseFloat(0); }
            if (E_NET_AMT == "") { E_NET_AMT = parseFloat(0); }
            if (RT == "") { RT = parseFloat(0); }
            var AMOUNT = 0;
            if (CALC_TYPE == "F") { AMOUNT = parseFloat(RT); }
            else if (CALC_TYPE == "P") {
                if (CALC_FORMULA == "A") { AMOUNT = parseFloat(A_NOS) * parseFloat(RT); }
                else if (CALC_FORMULA == "B") { AMOUNT = parseFloat(B_QNTY) * parseFloat(RT); }
                else if (CALC_FORMULA == "D") { AMOUNT = parseFloat(D_GROSS_AMT) * parseFloat(RT); }
                else if (CALC_FORMULA == "E") { AMOUNT = parseFloat(E_NET_AMT) * parseFloat(RT); }
            }
            document.getElementById("A_AMT_" + i).value = AMOUNT;
            var IGST_AMT = 0; var CGST_AMT = 0; var SGST_AMT = 0; var CESS_AMT = 0; var DUTY_AMT = 0;

            // IGST CALCULATION
            document.getElementById("AIGSTPER_" + i).value = IGST_PER;
            IGST_AMT = (AMOUNT * IGST_PER) / 100;

            AmountChange(document.getElementById("AIGSTAMT_" + i), document.getElementById("A_AMT_" + i), document.getElementById("AIGSTPER_" + i), document.getElementById("ANETAMT_" + i),
            document.getElementById("ACGSTAMT_" + i), document.getElementById("ASGSTAMT_" + i), document.getElementById("ACESSAMT_" + i), document.getElementById("ADUTYAMT_" + i));
            document.getElementById("AIGSTAMT_" + i).value = parseFloat(IGST_AMT).toFixed(2);
            //END

            // CGST CALCULATION
            document.getElementById("ACGSTPER_" + i).value = CGST_PER;
            CGST_AMT = (AMOUNT * CGST_PER) / 100;
            AmountChange(document.getElementById("ACGSTAMT_" + i), document.getElementById("A_AMT_" + i), document.getElementById("ACGSTPER_" + i), document.getElementById("ANETAMT_" + i),
                document.getElementById("AIGSTAMT_" + i), document.getElementById("ASGSTAMT_" + i), document.getElementById("ACESSAMT_" + i), document.getElementById("ADUTYAMT_" + i));
            document.getElementById("ACGSTAMT_" + i).value = parseFloat(CGST_AMT).toFixed(2);
            //END
            // SGST CALCULATION
            document.getElementById("ASGSTPER_" + i).value = SGST_PER;
            SGST_AMT = (AMOUNT * SGST_PER) / 100;
            AmountChange(document.getElementById("ASGSTAMT_" + i), document.getElementById("A_AMT_" + i), document.getElementById("ASGSTPER_" + i), document.getElementById("ANETAMT_" + i),
                document.getElementById("AIGSTAMT_" + i), document.getElementById("ASGSTAMT_" + i), document.getElementById("ACESSAMT_" + i), document.getElementById("ADUTYAMT_" + i));
            document.getElementById("ASGSTAMT_" + i).value = parseFloat(SGST_AMT).toFixed(2);
            //END
            // CESS CALCULATION
            document.getElementById("ACESSPER_" + i).value = CESS_PER;
            CESS_AMT = (AMOUNT * CESS_PER) / 100;
            AmountChange(document.getElementById("ACESSAMT_" + i), document.getElementById("A_AMT_" + i), document.getElementById("ACESSPER_" + i), document.getElementById("ANETAMT_" + i),
                document.getElementById("AIGSTAMT_" + i), document.getElementById("ASGSTAMT_" + i), document.getElementById("ACESSAMT_" + i), document.getElementById("ADUTYAMT_" + i));
            document.getElementById("ACESSAMT_" + i).value = parseFloat(CESS_AMT).toFixed(2);
            //END
            // DUTY CALCULATION

            document.getElementById("ADUTYPER_" + i).value = DUTY_PER;
            DUTY_AMT = (AMOUNT * DUTY_PER) / 100;
            AmountChange(document.getElementById("ADUTYAMT_" + i), document.getElementById("A_AMT_" + i), document.getElementById("ADUTYPER_" + i), document.getElementById("ANETAMT_" + i),
            document.getElementById("AIGSTAMT_" + i), document.getElementById("ASGSTAMT_" + i), document.getElementById("ACESSAMT_" + i), document.getElementById("ADUTYAMT_" + i));
            document.getElementById("ADUTYAMT_" + i).value = parseFloat(DUTY_AMT).toFixed(2);
            //END

            var NET_AMT = AMOUNT + parseFloat(document.getElementById("AIGSTAMT_" + i).value) + parseFloat(document.getElementById("ACGSTAMT_" + i).value) +
                         parseFloat(document.getElementById("ASGSTAMT_" + i).value) + parseFloat(document.getElementById("ACESSAMT_" + i).value) + parseFloat(document.getElementById("ADUTYAMT_" + i).value);
            document.getElementById("ANETAMT_" + i).value = parseFloat(NET_AMT).toFixed(2);

            //GRID TOTAL CALCULATION
            AmountCalculateTotal();
        }
    }
function AmountChange(id, AMOUNT, PER, NETAMT, AMT1, AMT2, AMT3, AMT4) {
    if ("@Model.DefaultAction" != "V") {
        var NEW_AMT = document.getElementById(id.id).value;
        var PERCENTAGE = document.getElementById(PER.id).value;
        var AMT = document.getElementById(AMOUNT.id).value;
        var AMT_1 = document.getElementById(AMT1.id).value;
        var AMT_2 = document.getElementById(AMT2.id).value;
        var AMT_3 = document.getElementById(AMT3.id).value;
        var AMT_4 = document.getElementById(AMT4.id).value;
        if (PERCENTAGE != "") {
            var CAL_ABET_AMT = parseFloat(AMT) * parseFloat(PERCENTAGE) / 100;
            var BAL_AMT = Math.abs(parseFloat(NEW_AMT) - parseFloat(CAL_ABET_AMT));
            if (BAL_AMT <= 1) {
                document.getElementById(id.id).value = parseFloat(NEW_AMT).toFixed(2);
                document.getElementById(NETAMT.id).value = parseFloat(parseFloat(NEW_AMT) + parseFloat(AMT) + parseFloat(AMT_1) + parseFloat(AMT_2) + parseFloat(AMT_3) + parseFloat(AMT_4)).toFixed(2);
            }
            else {
                document.getElementById(id.id).value = parseFloat(CAL_ABET_AMT).toFixed(2);
                document.getElementById(NETAMT.id).value = parseFloat(parseFloat(CAL_ABET_AMT) + parseFloat(AMT) + parseFloat(AMT_1) + parseFloat(AMT_2) + parseFloat(AMT_3) + parseFloat(AMT_4)).toFixed(2);
            }
        }
        //GRID TOTAL CALCULATION
        AmountCalculateTotal();
    }
}
function AmountCalculateTotal() {
    if ("@Model.DefaultAction" != "V") {
        var T_CURR_AMT = 0; var T_AMT = 0; var T_IGST_AMT = 0; var T_CGST_AMT = 0; var T_SGST_AMT = 0; var T_CESS_AMT = 0; var T_DUTY_AMT = 0; var T_NET_AMT = 0;
        var GridRow = $("#AMOUNT_GRID > tbody > tr").length;
        for (var i = 0; i <= GridRow - 1; i++) {
            var CURR_AMT = document.getElementById("ACURR_AMT_" + i).value;
            if (CURR_AMT != "") { T_CURR_AMT = T_CURR_AMT + parseFloat(CURR_AMT); } else { T_CURR_AMT = T_CURR_AMT + parseFloat(0); }

            var AMT = document.getElementById("A_AMT_" + i).value;
            if (AMT != "") { T_AMT = T_AMT + parseFloat(AMT); } else { T_AMT = T_AMT + parseFloat(0); }

            var IGST_AMT = document.getElementById("AIGSTAMT_" + i).value;
            if (IGST_AMT != "") { T_IGST_AMT = T_IGST_AMT + parseFloat(IGST_AMT); } else { T_IGST_AMT = T_IGST_AMT + parseFloat(0); }

            var CGST_AMT = document.getElementById("ACGSTAMT_" + i).value;
            if (CGST_AMT != "") { T_CGST_AMT = T_CGST_AMT + parseFloat(CGST_AMT); } else { T_CGST_AMT = T_CGST_AMT + parseFloat(0); }

            var SGST_AMT = document.getElementById("ASGSTAMT_" + i).value;
            if (SGST_AMT != "") { T_SGST_AMT = T_SGST_AMT + parseFloat(SGST_AMT); } else { T_SGST_AMT = T_SGST_AMT + parseFloat(0); }

            var CESS_AMT = document.getElementById("ACESSAMT_" + i).value;
            if (CESS_AMT != "") { T_CESS_AMT = T_CESS_AMT + parseFloat(CESS_AMT); } else { T_CESS_AMT = T_CESS_AMT + parseFloat(0); }

            var DUTY_AMT = document.getElementById("ADUTYAMT_" + i).value;
            if (DUTY_AMT != "") { T_DUTY_AMT = T_DUTY_AMT + parseFloat(DUTY_AMT); } else { T_DUTY_AMT = T_DUTY_AMT + parseFloat(0); }

            var NET_AMT = document.getElementById("ANETAMT_" + i).value;
            if (NET_AMT != "") { T_NET_AMT = T_NET_AMT + parseFloat(NET_AMT); } else { T_NET_AMT = T_NET_AMT + parseFloat(0); }

        }
        document.getElementById("A_T_CURR").value = parseFloat(T_CURR_AMT).toFixed(2);
        document.getElementById("A_T_AMOUNT").value = parseFloat(T_AMT).toFixed(2);
        document.getElementById("A_T_IGST").value = parseFloat(T_IGST_AMT).toFixed(2);
        document.getElementById("A_T_CGST").value = parseFloat(T_CGST_AMT).toFixed(2);
        document.getElementById("A_T_SGST").value = parseFloat(T_SGST_AMT).toFixed(2);
        document.getElementById("A_T_CESS").value = parseFloat(T_CESS_AMT).toFixed(2);
        document.getElementById("A_T_DUTY").value = parseFloat(T_DUTY_AMT).toFixed(2);
        document.getElementById("A_T_NET").value = parseFloat(T_NET_AMT).toFixed(2);

        //BILL AMOUNT CALCULATION
        BillAmountCalculate();
    }
}
function OpenAmount() {
    if ("@Model.DefaultAction" != "V") {
        var IGST_PER = 0; var CGST_PER = 0; var SGST_PER = 0; var CESS_PER = 0; var DUTY_PER = 0;
        var GridRowMain = $("#_T_SALE_DETAIL_GRID > tbody > tr").length;
        for (i = 0; i <= GridRowMain - 1; i++) {
            var IGST = parseFloat(document.getElementById("D_IGSTPER_" + i).value);
            var CGST = parseFloat(document.getElementById("D_CGSTPER_" + i).value);
            var SGST = parseFloat(document.getElementById("D_SGSTPER_" + i).value);
            var CESS = parseFloat(document.getElementById("D_CESSPER_" + i).value);
            var DUTY = 0;//parseFloat(document.getElementById("DUTYPER_" + i).value);
            if (IGST > IGST_PER) {
                IGST_PER = IGST;
            }
            if (CGST > CGST_PER) {
                CGST_PER = CGST;
            }
            if (SGST > SGST_PER) {
                SGST_PER = SGST;
            }
            if (CESS > CESS_PER) {
                CESS_PER = CESS;
            }
            if (DUTY > DUTY_PER) {
                DUTY_PER = DUTY;
            }
        }
        var GridRowMain = $("#AMOUNT_GRID > tbody > tr").length;
        for (i = 0; i <= GridRowMain - 1; i++) {
            document.getElementById("AIGSTPER_" + i).value = IGST_PER;
            document.getElementById("ACGSTPER_" + i).value = CGST_PER;
            document.getElementById("ASGSTPER_" + i).value = SGST_PER;
            document.getElementById("ACESSPER_" + i).value = CESS_PER;
            document.getElementById("ADUTYPER_" + i).value = DUTY_PER;
        }
    }
}
</script>
<script>
    function ReverceCharges() {
        if ("@Model.DefaultAction" != "V") {
            var GridRowMain = $("#_T_SALE_DETAIL_GRID > tbody > tr").length;
            var REVCHRG = $("#REVCHRG").val();
            for (var i = 0; i <= GridRowMain - 1; i++) {

                if (REVCHRG == "N") {
                    $("#D_IGSTPER_" + i).val(0);
                    $("#D_IGSTAMT_" + i).val(parseFloat(0).toFixed(2));
                    $("#D_CGSTPER_" + i).val(0);
                    $("#D_CGSTAMT_" + i).val(parseFloat(0).toFixed(2));
                    $("#D_SGSTPER_" + i).val(0);
                    $("#D_SGSTAMT_" + i).val(parseFloat(0).toFixed(2));
                    $("#D_CESSPER_" + i).val(0);
                    $("#D_CESSAMT_" + i).val(parseFloat(0).toFixed(2));
                    $("#D_IGSTPER_" + i).attr('readonly', 'readonly');
                    $("#D_IGSTAMT_" + i).attr('readonly', 'readonly');
                    $("#D_CGSTPER_" + i).attr('readonly', 'readonly');
                    $("#D_CGSTAMT_" + i).attr('readonly', 'readonly');
                    $("#D_SGSTPER_" + i).attr('readonly', 'readonly');
                    $("#D_SGSTAMT_" + i).attr('readonly', 'readonly');
                    $("#D_CESSPER_" + i).attr('readonly', 'readonly');
                    $("#D_CESSAMT_" + i).attr('readonly', 'readonly');
                    //var taxableamt = $("#D_TXBLVAL_" + i).val();
                    //var netamount = parseFloat(taxableamt).toFixed(2);
                    //$("#D_NETAMT_" + i).val(netamount);
                }
                else {
                    var taxableamt = $("#D_TXBLVAL_" + i).val();
                    $("#D_IGSTPER_" + i).removeAttr('readonly');
                    $("#D_IGSTAMT_" + i).removeAttr('readonly');
                    $("#D_CGSTPER_" + i).removeAttr('readonly');
                    $("#D_CGSTAMT_" + i).removeAttr('readonly');
                    $("#D_SGSTPER_" + i).removeAttr('readonly');
                    $("#D_SGSTAMT_" + i).removeAttr('readonly');
                    $("#D_CESSPER_" + i).removeAttr('readonly');
                    $("#D_CESSAMT_" + i).removeAttr('readonly');

                    var igstp = 0, cgstp = 0, sgstp = 0, cessper = 0;
                    prodgrpgstper = $("#D_PRODGRPGSTPER_" + i).val();
                    rate = $("#D_RATE_" + i).val();
                    if (rate == "") { rate = parseFloat(0); } else { rate = parseFloat(rate); }
                    var allgst = retGstPer(prodgrpgstper, rate);
                    if (allgst != "") {
                        var str = allgst.split(',');
                        if (str.length > 0) {
                            igstp = $(parseFloat(str[0]).toFixed(2)).val();
                            cgstp = $(parseFloat(str[1]).toFixed(2)).val();
                            sgstp = $(parseFloat(str[2]).toFixed(2)).val();
                        }

                        $("#D_IGSTPER_" + i).val(igstp);
                        //var igstamount = parseFloat(taxableamt * igstp / 100).toFixed(2);
                        //$("#D_IGSTAMT_" + i).val(igstamount);


                        $("#D_CGSTPER_" + i).val(cgstp);
                        //var cgstamount = parseFloat(taxableamt * cgstp / 100).toFixed(2);
                        //$("#D_CGSTAMT_" + i).val(cgstamount);


                        $("#D_SGSTPER_" + i).val(sgstp);
                        //var sgstamount = parseFloat(taxableamt * sgstp / 100).toFixed(2);
                        //$("#D_SGSTAMT_" + i).val(sgstamount);

                        //cessper = $("#BackupCESSTPER").val();
                        //$("#D_CESSPER_" + i).val(cessper);
                        //var cessamount = parseFloat(taxableamt * cessper / 100).toFixed(2);
                        //$("#D_CESSAMT_" + i).val(cessamount);
                        //var netamount = parseFloat(taxableamt) + parseFloat(igstamount) + parseFloat(cgstamount) + parseFloat(sgstamount) + parseFloat(cessamount);
                        //netamount = parseFloat(netamount).toFixed(2);
                        //$("#D_NETAMT_" + i).val(netamount);
                    }
                }
                CalculateAmt_Details(i);
            }
            //BillAmountCalculate();
            //DRCRBillAmount();
            //SBILLBillAmount();
        }
    }
function BillAmountCalculate() {
    if ("@Model.DefaultAction" != "V") {
        debugger;
        var R_TOTAL_BILL_AMOUNT = 0;
        var TOTAL_ROUND = 0;
        var netamt = 0;
        var ROUND_TAG = document.getElementById("RoundOff").checked;
        var D_TOTALNOS = 0, D_TOTALQNTY = 0, D_TOTALTAXVAL = 0, A_TOTALTAXVAL = 0, D_TOTALIGST = 0, A_TOTALIGST = 0, D_TOTALCGST = 0, A_TOTALCGST = 0, D_TOTALSGST = 0, A_TOTALSGST = 0, D_TOTALNETAMT = 0, A_TOTALNETAMT = 0;
        var T_NOS = $("#T_NOS").val();
        if (T_NOS != "") { D_TOTALNOS = D_TOTALNOS + parseFloat(T_NOS); } else { D_TOTALNOS = D_TOTALNOS + parseFloat(0); }

        var T_QNTY = $("#T_QNTY").val();
        if (T_QNTY != "") { D_TOTALQNTY = D_TOTALQNTY + parseFloat(T_QNTY); } else { D_TOTALQNTY = D_TOTALQNTY + parseFloat(0); }

        var T_GROSS_AMT = $("#T_GROSS_AMT").val();
        if (T_GROSS_AMT != "") { D_TOTALTAXVAL = D_TOTALTAXVAL + parseFloat(T_GROSS_AMT); } else { D_TOTALTAXVAL = D_TOTALTAXVAL + parseFloat(0); }

        var A_T_AMOUNT = $("#A_T_AMOUNT").val();
        if (A_T_AMOUNT != "") { A_TOTALTAXVAL = A_TOTALTAXVAL + parseFloat(A_T_AMOUNT); } else { A_TOTALTAXVAL = A_TOTALTAXVAL + parseFloat(0); }
        //
        var T_IGST_AMT = $("#T_IGST_AMT").val();
        if (T_IGST_AMT != "") { D_TOTALIGST = D_TOTALIGST + parseFloat(T_IGST_AMT); } else { D_TOTALIGST = D_TOTALIGST + parseFloat(0); }

        var A_T_IGST = $("#A_T_IGST").val();
        if (A_T_IGST != "") { A_TOTALIGST = A_TOTALIGST + parseFloat(A_T_IGST); } else { A_TOTALIGST = A_TOTALIGST + parseFloat(0); }

        var T_CGST_AMT = $("#T_CGST_AMT").val();
        if (T_CGST_AMT != "") { D_TOTALCGST = D_TOTALCGST + parseFloat(T_CGST_AMT); } else { D_TOTALCGST = D_TOTALCGST + parseFloat(0); }

        var A_T_CGST = $("#A_T_CGST").val();
        if (A_T_CGST != "") { A_TOTALCGST = A_TOTALCGST + parseFloat(A_T_CGST); } else { A_TOTALCGST = A_TOTALCGST + parseFloat(0); }

        var T_SGST_AMT = $("#T_SGST_AMT").val();
        if (T_SGST_AMT != "") { D_TOTALSGST = D_TOTALSGST + parseFloat(T_SGST_AMT); } else { D_TOTALSGST = D_TOTALSGST + parseFloat(0); }

        var A_T_SGST = $("#A_T_SGST").val();
        if (A_T_SGST != "") { A_TOTALSGST = A_TOTALSGST + parseFloat(A_T_SGST); } else { A_TOTALSGST = A_TOTALSGST + parseFloat(0); }

        var T_NET_AMT = $("#T_NET_AMT").val();
        if (T_NET_AMT != "") { D_TOTALNETAMT = D_TOTALNETAMT + parseFloat(T_NET_AMT); } else { D_TOTALNETAMT = D_TOTALNETAMT + parseFloat(0); }

        var A_T_NET = $("#A_T_NET").val();
        if (A_T_NET != "") { A_TOTALNETAMT = A_TOTALNETAMT + parseFloat(A_T_NET); } else { A_TOTALNETAMT = A_TOTALNETAMT + parseFloat(0); }

        var totaltaxval = 0;
        totaltaxval = parseFloat(parseFloat(D_TOTALTAXVAL) + parseFloat(A_TOTALTAXVAL)).toFixed(2);

        var totaltax = 0;
        totaltax = parseFloat(parseFloat(D_TOTALIGST) + parseFloat(A_TOTALIGST) + parseFloat(D_TOTALCGST) + parseFloat(A_TOTALCGST) + parseFloat(D_TOTALSGST) + parseFloat(A_TOTALSGST)).toFixed(2);

        var totalbillamt = 0;
        totalbillamt = parseFloat(parseFloat(D_TOTALNETAMT) + parseFloat(A_TOTALNETAMT)).toFixed(2);

        $("#TOTNOS").val(parseFloat(D_TOTALNOS).toFixed(0));
        $("#TOTQNTY").val(parseFloat(D_TOTALQNTY).toFixed(2));
        $("#TOTTAXVAL").val(parseFloat(totaltaxval).toFixed(2));
        $("#TOTTAX").val(parseFloat(totaltax).toFixed(2));


        var REVCHRG = $("#REVCHRG").val();
        if (REVCHRG == "Y") {
            totalbillamt = totalbillamt - totaltax;
        }
        if (ROUND_TAG == true) {
            R_TOTAL_BILL_AMOUNT = Math.round(totalbillamt);
            TOTAL_ROUND = R_TOTAL_BILL_AMOUNT - totalbillamt;
            document.getElementById("BLAMT").value = parseFloat(R_TOTAL_BILL_AMOUNT).toFixed(2);
            document.getElementById("ROAMT").value = parseFloat(TOTAL_ROUND).toFixed(2);
            @*if ("@Model.MENU_PARA" == "PI") {
                document.getElementById("BILL_AMTRecvd").value = parseFloat(R_TOTAL_BILL_AMOUNT).toFixed(2);
                var ADVRECDAMT = document.getElementById("ADVRECDAMT").value;
                if (ADVRECDAMT != "") {
                    document.getElementById("NET_AMTRecvd").value = (parseFloat(R_TOTAL_BILL_AMOUNT) - parseFloat(ADVRECDAMT)).toFixed(2);
                } else { document.getElementById("NET_AMTRecvd").value = ""; }

            }*@
            }
        else {
            //TOTAL_BILL_AMOUNT = parseFloat(PRODUCT_TOTAL_AMT) + parseFloat(AMOUNT_TOTAL_AMT);
            TOTAL_ROUND = 0;
            document.getElementById("BLAMT").value = parseFloat(totalbillamt).toFixed(2);
            @*if ("@Model.MENU_PARA" == "PI") {
                document.getElementById("BILL_AMTRecvd").value = parseFloat(totalbillamt).toFixed(2);
                var ADVRECDAMT = document.getElementById("ADVRECDAMT").value;
                if (ADVRECDAMT != "") { document.getElementById("NET_AMTRecvd").value = (parseFloat(totalbillamt) - parseFloat(ADVRECDAMT)).toFixed(2); }
                else { document.getElementById("NET_AMTRecvd").value = ""; }
            }*@
                document.getElementById("ROAMT").value = parseFloat(TOTAL_ROUND).toFixed(2);
        }
    }
}
</script>
<script>
    function CalulateTareWt(GRWT, NTWT, TRWT) {
        if ("@Model.DefaultAction" != "V") {
            var GROSS = document.getElementById(GRWT.id).value;
            if (GROSS == "") {
                GROSS = parseFloat(0);
            }
            var NET = document.getElementById(NTWT.id).value;
            if (NET == "") {
                NET = parseFloat(0);
            }
            var TARE = GROSS - NET;
            document.getElementById(TRWT.id).value = TARE.toFixed(3);
        }
    }
</script>
<script>
    function Checked_Disable() {
        var GridRow = $("#_T_SALE_PRODUCT_GRID > tbody > tr").length;
        for (var i = 0; i <= GridRow - 1; i++) {
            if ($("#B_ChildData_" + i).val() == "Y") {
                document.getElementById("B_Checked_" + i).disabled = true;
            }
        }
    }
</script>
<script>
    function DeleteRow() {
        if ("@Model.DefaultAction" != "V") {
            $.ajax({
                type: 'POST',
                url: "@Url.Action("DeleteRow", PageControllerName)",
                data: $('form').serialize(),
            success: function (result) {
                $("#partialdivBarCodeTab").animate({ marginTop: '0px' }, 50);
                $("#partialdivBarCodeTab").html(result);
                Checked_Disable();
                CalculateTotal_Barno();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $("#WaitingMode").hide();
                msgError(XMLHttpRequest.responseText);
                $("body span h1").remove(); $("#msgbody_error style").remove();
            }
        });
    }
}
</script>
<script>
    function AddDOCrow() {
        if ("@Model.DefaultAction" != "V") {
            $.ajax({
                type: 'POST',
                url: "@Url.Action("AddDOCRow", PageControllerName)",
                data: $('form').serialize(),
            success: function (result) {
                $("#partialdivDocument").animate({ marginTop: '-10px' }, 50);
                $("#partialdivDocument").html(result);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $("#WaitingMode").hide();
                msgError(XMLHttpRequest.responseText);
                $("body span h1").remove(); $("#msgbody_error style").remove();
            }
        });
    }
}
function DeleteDOCrow() {
    if ("@Model.DefaultAction" != "V") {
        $.ajax({
            type: 'POST',
            url: "@Url.Action("DeleteDOCRow", PageControllerName)",
            data: $('form').serialize(),
        success: function (result) {
            $("#partialdivDocument").animate({ marginTop: '0px' }, 50);
            $("#partialdivDocument").html(result);
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
}
}
</script>
<script>
    function SelectTDSCode(id, TDSHD, TDSNM, TCSPER) {
        if ("@Model.DefaultAction" != "V") {
            if (id == "") {
                $("#" + TDSHD.id).val("");
                $("#" + TDSNM.id).val("");
                $("#" + TCSPER.id).val("");
            }
            else {
                if (!emptyFieldCheck("Enter Document Date", "DOCDT")) { return false; }
                if (!emptyFieldCheck("Enter Supplier Code", "SLCD")) { return false; }
                var PARTY = document.getElementById("SLCD").value;
                var DOCDT = document.getElementById("DOCDT").value;
                $.ajax({
                    type: 'GET',
                    url: "@Url.Action("GetTDSDetails", PageControllerName)",
                    data:
                {
                    val: id,
                    PARTY: PARTY,
                    TAG: DOCDT
                },
                success: function (result) {
                    var MSG = result.indexOf(String.fromCharCode(181));
                    if (MSG >= 0) {
                        document.getElementById(TDSHD.id).value = returncolvalue(result, "TDSCODE");
                        document.getElementById(TDSNM.id).value = returncolvalue(result, "TDSNM");
                        var CMPNONCMP = returncolvalue(result, "CMPNONCMP");
                        if (CMPNONCMP == "L") {
                            document.getElementById(TCSPER.id).value = returncolvalue(result, "TDSPER");
                        } else {
                            document.getElementById(TCSPER.id).value = returncolvalue(result, "TDSPERNONCMP");
                        }
                    }
                    else {
                        $("#" + TDSHD.id).val("");
                        $("#" + TDSNM.id).val("");
                        $("#" + TCSPER.id).val("");
                        msgInfo(result);
                        message_value = TDSHD.id;
                    }
                },
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    $("#WaitingMode").hide();
                    msgError(XMLHttpRequest.responseText);
                    $("body span h1").remove(); $("#msgbody_error style").remove();
                }
            });
        }
    }
}
</script>
<script>
    function pageprinting() {
        debugger;
        var ty = $("#DOCCD").val();
        $.ajax({
            type: 'get',
            url: "@Url.Action("Print", PageControllerName)",
            data: "DOCNO=" + $("#DOCNO").val() + "&DOC_CD=" + $("#DOCCD").val() + "&DOCDT=" + $("#DOCDT").val(),
        success: function (result) {
            @*window.open("@Url.Action("Rep_OrdPrint", "Rep_OrdPrint", new { US = Model.UNQSNO_ENCRYPTED })", "_blank")*@
            window.open("@Url.Action("SaleBill_Print", "SaleBill_Print", new { US = Model.UNQSNO_ENCRYPTED })", "_blank")
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            $("#WaitingMode").hide();
            msgError(XMLHttpRequest.responseText);
            $("body span h1").remove(); $("#msgbody_error style").remove();
        }
    });
}
</script>