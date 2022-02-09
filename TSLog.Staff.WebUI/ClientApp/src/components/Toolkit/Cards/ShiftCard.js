import { Card } from '../_Meta/Card';

export class ShiftCard extends Card {
    static displayName = ShiftCard.name;

    componentDidMount() {
        this.SelectData();
    }

    render() {
        let card = super.render();

        return (
            <div>
                {card}
            </div>
        );
    }

    async SelectData() {
        const selCard = this.props.selectedData;

        const isNew = selCard === {};
        super.setState({
            cardType: "Смена",
            isNew: isNew,
            fieldList: {
                From: "От",
                To: "До",
                WorkMode: "Тип работы",
                Note: "Примечание"
            },
            fieldTypes: {
                From: "datetime",
                To: "datetime",
                WorkMode: "text",
                Note: "textarea"
            },
            currentValues: selCard,
            syncVal: this.props.updateSetValCallback,
        });


        //currentValues: {
        //        From: "2022-02-04T00:03:22",
        //        Id: 100004,
        //        Note: "hui",
        //        To: "2022-02-04T06:03:22"
        //    }
    }
}
