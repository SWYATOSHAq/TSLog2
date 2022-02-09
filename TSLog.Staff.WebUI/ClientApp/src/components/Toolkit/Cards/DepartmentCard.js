import { Card } from '../_Meta/Card';

export class DepartmentCard extends Card {
    static displayName = DepartmentCard.name;
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
            cardType: "Подразделение",
            isNew: isNew,
            fieldList: {
                Name: "Наименование",
                ShortName: "Кор. наименование",
                Code: "Код",
                IsActive: "Активен",
            },
            fieldTypes: {
                Name: "text",
                ShortName: "text",
                Code: "text",
                IsActive: "text",
            },
            currentValues: selCard,
            syncVal: this.props.updateSetValCallback,
        });
    }
}
