import { Card } from '../_Meta/Card';

export class EmployeesCard extends Card {
    static displayName = EmployeesCard.name;

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
            cardType: "Работник",
            isNew: isNew,
            fieldList: {
                FirstName: "Имя",
                Patronymic: "Фамилия",
                Nationality: "Национальность",
                Sex: "Пол",
                BirthDate: "Дата рождения",
                Email: "Почта",
            },
            fieldTypes: {

                FirstName: "text",
                Patronymic: "text",
                Nationality: "text",
                Sex: "text",
                BirthDate: "datetime",
                Email: "text",
            },
            currentValues: selCard,
            syncVal: this.props.updateSetValCallback,
        });
    }
}
