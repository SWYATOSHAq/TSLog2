/*
  Страница отчёта по загруженности склада (#2198 WEB отчёт по загруженности склада. Первый приоритет.).
  Автор - Буров Михаил.
  Создано - 2022.09.08.
*/

// Класс страницы отчёта по загруженности склада
class WarehouseFillingReportPage {
    shortUrl;

    data = [];
    busy = 0;
    total = 0;

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
        let fill = 0;
        let free = 0;
        let busy = 0;
        let total = 0;
        let freePlaces = 0;

        const blockFillingDiv = document.getElementById(area + "-filling-div")
        fill = Math.round(this.data[area + "_percent"] * 100);
        free = 100 - fill;
        busy = this.data[area + "_busy"];
        total = this.data[area + "_total"]
        this.busy += busy;
        this.total += total;        
        freePlaces = total - busy;

        blockFillingDiv.style.cssText = "background-color: " + color + "; height: " + fill + "%; margin-top: " + free + "%";

        const captionPercentDiv = document.getElementById(area + "-caption-percent-div");
        captionPercentDiv.textContent = fill + " %";

        const loadCostInput = document.getElementById(area + "-load-cost-input");
        const turnoverKIput = document.getElementById(area + "-turnover-K-input");
        const priceIput = document.getElementById(area + "-price-input");
        const loadCost = Number(loadCostInput.value);
        const turnoverK = Number(turnoverKIput.value);
        const price = Number(priceIput.value);
        const dayIncome = Math.round(busy * price + busy * loadCost * turnoverK / 30.0);
        const captionIncomeDiv = document.getElementById(area + "-caption-income-div");
        captionIncomeDiv.textContent = utils.displayFloat(dayIncome) + " руб."; 

        const captionValueDiv = document.getElementById(area + "-caption-value-div")
        captionValueDiv.textContent = "свободно " + utils.displayFloat(freePlaces);
        //captionValueDiv.textContent = utils.displayFloat(busy) + " из " + utils.displayFloat(total);
    }

    showAllBlock(color) {
        let fill = 0;
        let free = 0;        
        let freePlaces = 0;

        fill = Math.round(this.busy / this.total * 100);
        free = 100 - fill;
        freePlaces = this.total - this.busy;

        const blockFillingDiv = document.getElementById("ALL-filling-div")
        blockFillingDiv.style.cssText = "background-color: " + color + "; height: " + fill + "%; margin-top: " + free + "%";

        const captionPercentDiv = document.getElementById("ALL-caption-percent-div");
        captionPercentDiv.textContent = fill + " %";

        const captionValueDiv = document.getElementById("ALL-caption-value-div")
        captionValueDiv.textContent = "свободно " + utils.displayFloat(freePlaces);
    }

    showReport() {
        this.busy = 0;
        this.total = 0;   

        this.showBlock("HBW", "lightskyblue");
        this.showBlock("LBW", "lightskyblue");
        this.showBlock("LB1_1_2", "lightskyblue");
        this.showBlock("P01", "lightskyblue");

        this.showBlock("VKU", "bisque");
        this.showBlock("LM2", "bisque");
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