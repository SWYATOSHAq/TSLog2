import { Card } from '../_Meta/Card';

export class PositionCard extends Card {
    static displayName = PositionCard.name;

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
            cardType: "Должность",
            isNew: isNew,
            fieldList: {
                Name: "Наименование",
                Code: "Код",
                IsActive: "Активен",
                IsLinear: "Линеен",
                LaborHourCost: "Стоимость часа",
            },
            fieldTypes: {
                Name: "text",
                Code: "text",
                IsActive: "bool",
                IsLinear: "bool",
                LaborHourCost: "int",
            },
            currentValues: selCard,
            syncVal: this.props.updateSetValCallback,
        });
    }
}
