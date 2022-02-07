import { Card } from '../_Meta/Card';

export class DepartmentCard extends Card {
    static displayName = DepartmentCard.name;

    constructor(props) {
        super(props);
    }


    componentDidMount() {
        this.SelectData();
    }

    render() {
        let card = super.render();

        return (
            <div key={this.props.selectedData}>
                fffffffff {JSON.stringify(this.props.selectedData)}
                <p>ShiftCard</p>
                {card}

            </div>
        );
    }

    async SelectData() {
        //const response = await fetch('/api/v1/Staff/Shifts');
        //const data = await response.json();
        //console.log(data);
        //const selCard = this.props.selCard;
        //console.log(selCard);

        const selCard = {};
        //const selCard = this.props.selectedData;

        const isNew = selCard == {};
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
