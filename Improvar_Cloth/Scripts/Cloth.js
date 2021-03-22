﻿var DefaultAction = $("#DefaultAction").val();
function retGstPerstr(prodgrpgstper, rate) {
    debugger;
    if (DefaultAction == "V") return true;
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
function retGstPer(prodgrpgstper, rate) {
    var gstRate = retGstPerstr(prodgrpgstper, rate);
    var gstarr = gstRate.split(',');
    gstRate = retFloat(gstarr[0]) + retFloat(gstarr[1]) + retFloat(gstarr[2]);
    return gstRate;
}
$(document).on('click', '.arrow-left, .arrow-right', function () {
    var next;
    var circler;
    if ($(this).is('.arrow-left')) {
        next = 'prev';
        circler = ':last';
    } else {     // or if there would be more arrows, use : else if ($(this).is('.arrow-right'))
        next = 'next';
        circler = ':first';
    }
    var nextTarget = $('#div_carousel_inner div.active')[next]('div');
    if (nextTarget.length == 0) {
        nextTarget = $('#div_carousel_inner div' + circler);
    }
    $('#div_carousel_inner div').removeClass('active');
    nextTarget.addClass('active');
});
function FillImageModal(FieldId) {
    var actt = ""; $("#div_carousel_inner").html('');
    var arr = $("#" + FieldId).val();
    arr = arr.split(String.fromCharCode(179));
    $.each(arr, function (index, value) {
        var imgname = (value.split('~')[0]);
        var id = (imgname).split('.')[0];
        var ImageDesc = (value.split('~')[1]);
        var htm = ''; if (index == 0) { actt = "active"; } else { actt = ""; }
        htm += '<div id="' + id + '" class="item ' + actt + '">';
        htm += '    <img src="/UploadDocuments/' + imgname + '"  alt="Img Not Found" style="width:100%;">';
        htm += '    <span class="carousel-caption">';
        htm += '    <p> ' + ImageDesc + ' </p>';
        htm += '    </span>';
        htm += '</div>';
        $("#div_carousel_inner").append(htm);
    });
}
function CalculateDiscount(DiscTypeId, DiscRateId, QntyId, NosId, GrossAmtId, DiscountedAmt) {
    var DISCAMT = 0;
    var DiscType = $("#" + DiscTypeId).val();
    var DiscRate = retFloat($("#" + DiscRateId).val());
    if (DiscType == "Q") {
        var Qnty = retFloat($("#" + QntyId).val());
        DISCAMT = DiscRate * Qnty;
    }
    else if (DiscType == "N") {
        var Nos = retFloat($("#" + NosId).val());
        DISCAMT = DiscRate * Nos;
    }
    else if (DiscType == "P") {
        var Amt = retFloat($("#" + GrossAmtId).val());
        DISCAMT = (DiscRate * Amt) / 100;
    }
    else if (DiscType == "F") { DISCAMT = DiscRate; }
        //else if (DiscType == "A") { DISCAMT = DiscRate; }
    else if (DiscType == "A") {
        DiscountedAmt = retFloat(DiscountedAmt);
        DISCAMT = (DiscRate * DiscountedAmt) / 100;
    }
    else { DISCAMT = 0; }
    DISCAMT = parseFloat(DISCAMT).toFixed(2);
    return parseFloat(DISCAMT);
}
function CopyLastDiscData(RATE, TYPE, RATE_ID, TYPE_ID, ITCD_ID, TABLENM) {
    const keyName = event.key;
    if (keyName == "F4") {
        var DISCTYPE_DESC = TYPE == "P" ? "%" : TYPE == "N" ? "Nos" : TYPE == "Q" ? "Qnty" : TYPE == "A" ? "AftDsc%" : TYPE == "F" ? "Fixed" : "";
        var GridRow = $("#" + TABLENM + " > tbody > tr").length;
        for (var i = 0; i <= GridRow - 1; i++) {
            if (retStr($("#" + ITCD_ID + i).val()) != "") {
                var RA_TE = retFloat($("#" + RATE_ID + i).val());
                if (RA_TE == 0) {
                    $("#" + RATE_ID + i).val(RATE);
                    $("#" + TYPE_ID + i).val(TYPE);
                    $("#" + TYPE_ID + "DESC_" + i).val(DISCTYPE_DESC);
                }
                $("#" + RATE_ID + i).blur();
            }
        }
    }
}
function RemoveLastDiscData(RATEID, ITCD_ID, TABLENM) {
    const keyName = event.key;
    if (keyName == "F9") {
        var GridRow = $("#" + TABLENM + " > tbody > tr").length;
        for (var i = 0; i <= GridRow - 1; i++) {
            if (retStr($("#" + ITCD_ID + i).val()) != "") {
                $("#" + RATEID + i).val(parseFloat(0).toFixed(2));
                $("#" + RATEID + i).blur();
            }
        }
    }
}
function CharmPrice(ChrmType, Rate, RoundVal) {
    debugger;
    RoundVal = RoundVal.padStart(2, '0');
    if (Rate < 10) Rate = 10 + Rate;
    var RoundAmt = retInt(RoundVal);
    if (RoundAmt == 0) RoundAmt = 100;
    var small = retInt(retInt(Rate / RoundAmt) * RoundAmt);
    var big = retInt(small + RoundAmt);
    if (ChrmType == "RD")//ROUND
    {
        var roundValu = retInt((Rate - small >= big - Rate) ? big : small);
        return roundValu;
    }
    else if (ChrmType == "RN")//ROUNDNEXT
    {
        return big;
    }
    else if (ChrmType == "NT")//NEXT
    {
        var last2rate = retInt(Rate % 100);
        if (last2rate <= retInt(RoundVal)) {
            big = retInt(retStr(Rate).substring(0, retStr(Rate).length - 2) + RoundVal);
        }
        else {
            big = retInt(retStr(Rate + 100).substring(0, retStr(Rate + 100).length - 2) + RoundVal);
        }
        return retInt(big);
    }
    else if (ChrmType == "NR")//NEAR
    {
        var last2rate = retInt(Rate % 100);
        if (last2rate <= retInt(RoundVal)) {
            big = retInt(retStr(Rate).substring(0, retStr(Rate).length - 2) + RoundVal);
            small = retInt(retStr(Rate - 100).substring(0, retStr(Rate - 100).length - 2) + RoundVal);
        }
        else {
            big = retInt(retStr(Rate + 100).substring(0, retStr(Rate + 100).length - 2) + RoundVal);
            small = retInt(retStr(Rate).substring(0, retStr(Rate).length - 2) + RoundVal);
        }
        var NEAR = retInt((Rate - small >= big - Rate) ? big : small);
        return retInt(NEAR);
    }
    else {
        return 0;
    }
}
