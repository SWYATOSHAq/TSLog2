class OperationPrintPage {
    show() {
        if ($("#force-print").val())
            window.print();
    }
}

const operationPrintPage = new OperationPrintPage();

document.addEventListener('DOMContentLoaded', operationPrintPage.show.bind(operationPrintPage));