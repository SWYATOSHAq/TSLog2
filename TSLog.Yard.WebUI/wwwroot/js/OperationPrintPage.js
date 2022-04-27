class OperationPrintPage {
    show() {
        if ($("#force-print").val())
            window.print();
    }
}

const operationPrintPage = new OperationPrintPage();

$(document).ready(operationPrintPage.show.bind(operationPrintPage));