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

        const isNew = Object.keys(selCard).length === 0;
        super.setState({
            cardType: "Работник",
            isNew: isNew,
            fieldList: {
                Surname: "Фамилия",
                FirstName: "Имя",
                Patronymic: "Отчество",
                Sex: "Пол",
                Company: "Компания",
                BirthDate: "Дата рождения",
                Nationality: "Национальность",
                Email: "Почта",
                DocumentName: "Наим. документа",
                DocumentNumber: "Номер документа",
                DocumentSeries: "Серия документа",
                DocumentIssueOrganization: "Орг. выдав. документ",
                DocumentIssueDate: "Дата выдачи документа",
                Positions: "Должности",
            },
            fieldTypes: {

                Surname: "text",
                FirstName: "text",
                Patronymic: "text",
                Nationality: "text",
                Sex: "gender",
                Company: "company",
                BirthDate: "date",
                Email: "text",
                DocumentName: "text",
                DocumentNumber: "text",
                DocumentSeries: "text",
                DocumentIssueOrganization: "text",
                DocumentIssueDate: "date",
                Positions: "positions",
                
            },
            currentValues: selCard,
            syncVal: this.props.updateSetValCallback,
        });
    }
}
