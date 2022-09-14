/*
  Страница отчёта по загруженности склада (#2198 WEB отчёт по загруженности склада. Первый приоритет.).
  Автор - Буров Михаил.
  Создано - 2022.09.08.
*/

// Класс страницы отчёта по загруженности склада
class WarehouseFillingReportPage {
    shortUrl;

    data = [];
    busyPallets = 0;
    busyPieces = 0;
    totalPallets = 0;
    totalPieces = 0;

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    createParameters() {
        let result = "";
        return result != "" ? "?" + result : "";
    }

    async getData() {
        const result = await api.getWarehouseFillingReportData();
        if (result.code === 0) {
            this.data = result.data;
        } else {
            viewUtils.showError(result.message);
        }
    }

    showBlock(area, color) {
        let busyPercent = 0;
        let freePercent = 0;
        let busy = 0;
        let total = 0;
        let free = 0;

        const blockFillingDiv = document.getElementById(area + "-filling-div")
        busyPercent = Math.round(this.data[area + "_percent"] * 100);
        freePercent = 100 - busyPercent;
        busy = this.data[area + "_busy"];
        total = this.data[area + "_total"]
        this.busyPallets += this.data[area + "_busy_pallets"];
        this.busyPieces += this.data[area + "_busy_pieces"];
        this.totalPallets += this.data[area + "_total_pallets"];
        this.totalPieces += this.data[area + "_total_pieces"];
        free = total - busy;

        blockFillingDiv.style.cssText = "background-color: " + color + "; height: " + busyPercent + "%; margin-top: " + freePercent + "%";

        const captionPercentDiv = document.getElementById(area + "-caption-percent-div");
        captionPercentDiv.textContent = busyPercent + " %";

        const loadCostInput = document.getElementById(area + "-load-cost-input");
        const turnoverKIput = document.getElementById(area + "-turnover-K-input");
        const priceIput = document.getElementById(area + "-price-input");
        const loadCost = Number(loadCostInput.value);
        const turnoverK = Number(turnoverKIput.value);
        const price = Number(priceIput.value);
        const dayIncome = Math.round(free * (price * 30.5 + loadCost * turnoverK));
        const captionIncomeDiv = document.getElementById(area + "-caption-income-div");
        captionIncomeDiv.textContent = utils.displayFloat(dayIncome) + " руб."; 

        const captionPalletsDiv = document.getElementById(area + "-caption-pallets-div")
        captionPalletsDiv.textContent = "св. палл. " + utils.displayFloat(this.data[area + "_total_pallets"] - this.data[area + "_busy_pallets"]);

        const captionPiecesDiv = document.getElementById(area + "-caption-pieces-div")
        captionPiecesDiv.textContent = "св. яч-к " + utils.displayFloat(this.data[area + "_total_pieces"] - this.data[area + "_busy_pieces"]);
        //captionValueDiv.textContent = utils.displayFloat(busy) + " из " + utils.displayFloat(total);
    }

    showAllBlock(color) {
        let busyPercent = 0;
        let freePercent = 0;  
        let busy = this.busyPallets + this.busyPieces;
        let total = this.totalPallets + this.totalPieces;

        busyPercent = Math.round(busy / total * 100);
        freePercent = 100 - busyPercent;

        const blockFillingDiv = document.getElementById("ALL-filling-div")
        blockFillingDiv.style.cssText = "background-color: " + color + "; height: " + busyPercent + "%; margin-top: " + freePercent + "%";

        const captionPercentDiv = document.getElementById("ALL-caption-percent-div");
        captionPercentDiv.textContent = busyPercent + " %";

        const captionPalletsDiv = document.getElementById("ALL-caption-pallets-div")
        captionPalletsDiv.textContent = "св. палл. " + utils.displayFloat(this.totalPallets - this.busyPallets);

        const captionPiecesDiv = document.getElementById("ALL-caption-pieces-div")
        captionPiecesDiv.textContent = "св. яч-к " + utils.displayFloat(this.totalPieces - this.busyPieces);
    }

    showReport() {
        this.busyPallets = 0;
        this.busyPieces = 0;
        this.totalPallets = 0;   
        this.totalPieces = 0;   

        this.showBlock("HBW", "lightskyblue");
        this.showBlock("LBW", "lightskyblue");
        this.showBlock("LB1_1_2", "lightskyblue");
        this.showBlock("P01", "lightskyblue");

        this.showBlock("VKU", "bisque");
        this.showBlock("LM2", "bisque");
        this.showBlock("LF2", "bisque");
        this.showBlock("LB1_3_E", "bisque");
        this.showBlock("P02", "bisque");

        this.showBlock("LF3", "lightpink");
        this.showBlock("P03", "lightpink");

        this.showAllBlock("lightpink");
    }

    print() {
        window.print();
    }

    async show() {
        await this.getData();

        this.showReport();
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }
}

const warehouseFillingReportPage = new WarehouseFillingReportPage();

document.addEventListener('DOMContentLoaded', warehouseFillingReportPage.show.bind(warehouseFillingReportPage));