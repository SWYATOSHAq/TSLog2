/*
  Страница отчёта по загруженности склада (#2198 WEB отчёт по загруженности склада. Первый приоритет.).
  Автор - Буров Михаил.
  Создано - 2022.09.08.
*/

// Класс страницы отчёта по загруженности склада
class WarehouseFillingReportPage {
    shortUrl;

    data1 = [];
    data2 = [];
    busyPallets = 0;
    busyPieces = 0;
    totalPallets = 0;
    totalPieces = 0;
    shortfall = 0;
    //areas = ["HBW", "LBW", "LB1_1_2", "P01", "VKU", "LM2", "LF2", "LB1_3_E", "P02", "LF3", "P03"]
    areas = ["HBW", "LBW", "LB1_1_2", "P01", "VKU", "LF2", "LB1_3_E", "P02", "LF3", "P03"]

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    createParameters() {
        let result = "";
        result = utils.addParameter(result, "from_date", "from-date-input");
        result = utils.addParameter(result, "to_date", "to-date-input");
        return result != "" ? "?" + result : "";
    }

    // Получение данных с сервера по rest api
    async getData1() {
        const result = await api.getWarehouseFillingReportData1();
        if (result.code === 0) {
            this.data1 = result.data;
        } else {
            viewUtils.showError(result.message);
        }
    }

    async getData2(fromDate, toDate) {
        const result = await api.getWarehouseFillingReportData2(fromDate, toDate);
        if (result.code === 0) {
            this.data2 = result.data;
        } else {
            viewUtils.showError(result.message);
        }
    }

    // Вычисляет общёю недополучку по ВСЕМ областям склада
    calculateTotalShortfall() {
        this.shortfall = 0;
        for (let area of this.areas) {
            const captionShortfallDiv = document.getElementById(area + "-caption-income-div");
            const shortfall = Number(captionShortfallDiv.getAttribute("shortfall"));
            this.shortfall += shortfall;
        }
        const captionShortfallDiv = document.getElementById("ALL-caption-income-div");
        captionShortfallDiv.textContent = utils.displayFloat(this.shortfall) + " руб."; 
    }

    // Создаёт функцию обработчик события при изменении параметров (для определения дохода) области склада
    createOnchangeFunction(freePallets, freePieces,
        palletLoadCostInput, palletTurnoverKIput, palletPriceIput,
        pieceLoadCostInput, pieceTurnoverKIput, piecePriceIput,
        captionShortfallDiv)
    {
        return function () {
            const palletLoadCost = Number(palletLoadCostInput.value);
            const palletTurnoverK = Number(palletTurnoverKIput.value);
            const palletPrice = Number(palletPriceIput.value);
            const pieceLoadCost = Number(pieceLoadCostInput.value);
            const pieceTurnoverK = Number(pieceTurnoverKIput.value);
            const piecePrice = Number(piecePriceIput.value);

            const dayShortfall = Math.round(freePallets * (palletPrice * 30.5 + palletLoadCost * palletTurnoverK) + 
                freePieces * (piecePrice * 30.5 + pieceLoadCost * pieceTurnoverK));
            captionShortfallDiv.textContent = utils.displayFloat(dayShortfall) + " руб."; 
            captionShortfallDiv.setAttribute("shortfall", dayShortfall);

            this.calculateTotalShortfall();
        }
    }

    // Отображает сведения по области склада
    showBlock(area, color) {
        let busyPercent = 0;
        let freePercent = 0;
        let busyPallets = 0;
        let busyPieces = 0;
        let totalPallets = 0;
        let totalPieces = 0;
        let freePallets = 0;
        let freePieces = 0;

        const blockFillingDiv = document.getElementById(area + "-filling-div")
        busyPercent = Math.round(this.data1[area + "_percent"] * 100);
        freePercent = 100 - busyPercent;
        
        busyPallets = this.data1[area + "_busy_pallets"];
        busyPieces = this.data1[area + "_busy_pieces"];
        totalPallets = this.data1[area + "_total_pallets"];
        totalPieces = this.data1[area + "_total_pieces"]

        this.busyPallets += busyPallets;
        this.busyPieces += busyPieces;
        this.totalPallets += totalPallets;
        this.totalPieces += totalPieces;
        freePallets = totalPallets - busyPallets;
        freePieces = totalPieces - busyPieces;

        blockFillingDiv.style.cssText = "background-color: " + color + "; height: " + busyPercent + "%; margin-top: " + freePercent + "%";

        const captionPercentDiv = document.getElementById(area + "-caption-percent-div");
        captionPercentDiv.textContent = "Занято - " + busyPercent + " %";

        const palletLoadCostInput = document.getElementById(area + "-pallet-load-cost-input");
        const palletTurnoverKIput = document.getElementById(area + "-pallet-turnover-K-input");
        const palletPriceIput = document.getElementById(area + "-pallet-price-input");
        const pieceLoadCostInput = document.getElementById(area + "-piece-load-cost-input");
        const pieceTurnoverKIput = document.getElementById(area + "-piece-turnover-K-input");
        const piecePriceIput = document.getElementById(area + "-piece-price-input");
        const captionShortfallDiv = document.getElementById(area + "-caption-income-div");

        const onchangeFunction = this.createOnchangeFunction(freePallets, freePieces,
            palletLoadCostInput, palletTurnoverKIput, palletPriceIput,
            pieceLoadCostInput, pieceTurnoverKIput, piecePriceIput,
            captionShortfallDiv).bind(this);
        palletLoadCostInput.onchange = onchangeFunction;
        palletTurnoverKIput.onchange = onchangeFunction;
        palletPriceIput.onchange = onchangeFunction;
        pieceLoadCostInput.onchange = onchangeFunction;
        pieceTurnoverKIput.onchange = onchangeFunction;
        piecePriceIput.onchange = onchangeFunction;

        onchangeFunction();

        /*
        const loadCost = Number(palletLoadCostInput.value);
        const turnoverK = Number(palletTurnoverKIput.value);
        const price = Number(palletPriceIput.value);
        const dayShortfall = Math.round(free * (price * 30.5 + loadCost * turnoverK));
        this.shortfall += dayShortfall;
        captionShortfallDiv.textContent = utils.displayFloat(dayShortfall) + " руб."; 
        captionShortfallDiv.setAttribute("shortfall", dayShortfall);
        */

        const captionPalletsDiv = document.getElementById(area + "-caption-pallets-div")
        captionPalletsDiv.textContent = "св. палл. " + utils.displayFloat(this.data1[area + "_total_pallets"] - this.data1[area + "_busy_pallets"]);

        const captionPiecesDiv = document.getElementById(area + "-caption-pieces-div")
        captionPiecesDiv.textContent = "св. яч-к " + utils.displayFloat(this.data1[area + "_total_pieces"] - this.data1[area + "_busy_pieces"]);
        //captionValueDiv.textContent = utils.displayFloat(busy) + " из " + utils.displayFloat(total);
    }

    // Отображает сведения по ВСЕМ областям склада
    showAllBlock(color) {
        let busyPercent = 0;
        let freePercent = 0;  
        let busy = this.busyPallets + this.busyPieces;
        let total = this.totalPallets + this.totalPieces;

        busyPercent = Math.round(busy / total * 100);
        freePercent = 100 - busyPercent;

        const captionShortfallDiv = document.getElementById("ALL-caption-income-div");
        captionShortfallDiv.textContent = utils.displayFloat(this.shortfall) + " руб."; 

        const blockFillingDiv = document.getElementById("ALL-filling-div")
        blockFillingDiv.style.cssText = "background-color: " + color + "; height: " + busyPercent + "%; margin-top: " + freePercent + "%";

        const captionPercentDiv = document.getElementById("ALL-caption-percent-div");
        captionPercentDiv.textContent = "Занято - " + busyPercent + " %";

        const captionPalletsDiv = document.getElementById("ALL-caption-pallets-div")
        captionPalletsDiv.textContent = "св. палл. " + utils.displayFloat(this.totalPallets - this.busyPallets);

        const captionPiecesDiv = document.getElementById("ALL-caption-pieces-div")
        captionPiecesDiv.textContent = "св. яч-к " + utils.displayFloat(this.totalPieces - this.busyPieces);
    }

    // Отображение заполняемости склада по областям
    showPart1() {
        this.busyPallets = 0;
        this.busyPieces = 0;
        this.totalPallets = 0;
        this.totalPieces = 0;
        this.shortfall = 0;

        this.showBlock("HBW", "lightskyblue");
        this.showBlock("LBW", "lightskyblue");
        this.showBlock("LB1_1_2", "lightskyblue");
        this.showBlock("P01", "lightskyblue");

        this.showBlock("VKU", "bisque");
        //this.showBlock("LM2", "bisque");
        this.showBlock("LF2", "bisque");
        this.showBlock("LB1_3_E", "bisque");
        this.showBlock("P02", "bisque");

        this.showBlock("LF3", "lightpink");
        this.showBlock("P03", "lightpink");

        this.showAllBlock("lightpink");
    }

    // Отображение графика заполняемости склада
    showPart2() {
        const daysCount = this.data2.length;
        const canvas = document.getElementById("canvas");
        canvas.style.cssText = "border: 1px solid black; width: " + (daysCount * 5 + 80) + "px; height: 580px";
        canvas.setAttribute("width", daysCount * 5 + 80);
        canvas.setAttribute("height", 580);

        const context = canvas.getContext('2d');

        context.fillStyle = "black";
        context.font = context.font.replace(/\d+px/, "12px");
        context.lineWidth = 2.0;
        context.beginPath();
        context.moveTo(40, 40);
        context.lineTo(40, 548);
        context.moveTo(40, 540);
        context.lineTo(40 + daysCount * 5, 540);
        context.stroke();

        context.fillStyle = "black";
        for (let i = 0; i < 11; i++) {
            context.fillText((10 - i) * 10 + "", 8, i * 50 + 44);
            context.beginPath();
            context.moveTo(32, i * 50 + 40);
            context.lineTo(40, i * 50 + 40);
            context.stroke();
        }

        let i = 0;
        let x = 0;
        let y = 0;
        let prevX = 0;
        let prevY = 0;
        const capacity = this.totalPallets + this.totalPieces;
        for (let element of this.data2) {
            i++;
            prevX = x;
            prevY = y;
            x = 40 + (i - 1) * 5;
            y = 40 + (100 - Math.round(element.busy / capacity * 100)) * 5;
            if (i > 1) {
                context.beginPath();
                context.moveTo(prevX, prevY);
                context.lineTo(x, y);
                context.stroke();
            }

            // yyyy.MM.dd
            const day = element.date.substring(8, 10);
            if (day === "01") {
                const caption = element.date.substring(5, 10)
                context.fillText(caption, x - 15, 560);

                context.beginPath();
                context.moveTo(x, 540);
                context.lineTo(x, 548);
                context.stroke();
            }
        }

        if (this.data2.length > 0) {
            let caption = this.data2[0].date.substring(5, 10)
            context.fillText(caption, 40 - 15, 574);

            caption = this.data2[this.data2.length - 1].date.substring(5, 10)
            context.fillText(caption, 40 + daysCount * 5 - 15, 574);

            context.beginPath();
            context.moveTo(40 + daysCount * 5, 540);
            context.lineTo(40 + daysCount * 5, 548);
            context.stroke();
        }
    }

    showReport() {
        this.showPart1();
        this.showPart2();
    }

    async createReport() {
        const fromDate = utils.getUrlParameter("from_date");
        const toDate = utils.getUrlParameter("to_date");

        await this.getData1();
        await this.getData2(fromDate, toDate);

        this.showReport();
    }

    print() {
        window.print();
    }

    onChangeDate() {
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }

    async show() {
        const fromDatePicker = $("#from-date-input");
        const toDatePicker = $("#to-date-input");
        fromDatePicker.datepicker({
            format: "dd.mm.yyyy",
            autoclose: true,
            language: "ru",
            todayBtn: true,
            todayHighlight: true,
        });
        toDatePicker.datepicker({
            format: "dd.mm.yyyy",
            autoclose: true,
            language: "ru",
            todayBtn: true,
            todayHighlight: true,
        });

        let fromDate = utils.getUrlParameter("from_date");
        let toDate = utils.getUrlParameter("to_date");
        if (fromDate == "") {
            fromDate = utils.getNowDate();
        }
        fromDatePicker.datepicker("setDate", fromDate);
        fromDatePicker.on("changeDate", this.onChangeDate.bind(this));
        if (toDate == "") {
            toDate = fromDate;
        }
        toDatePicker.datepicker("setDate", toDate);
        toDatePicker.on("changeDate", this.onChangeDate.bind(this));

        await this.createReport();
       
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }
}

const warehouseFillingReportPage = new WarehouseFillingReportPage();

document.addEventListener('DOMContentLoaded', warehouseFillingReportPage.show.bind(warehouseFillingReportPage));