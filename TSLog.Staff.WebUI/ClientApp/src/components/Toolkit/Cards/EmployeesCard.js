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
                Surname: "Фамилия",
                Name: "Имя",
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

            },
            fieldTypes: {

                Surname: "text",
                Name: "text",
                Patronymic: "text",
                Nationality: "text",
                Sex: "text",
                Company: "company",
                BirthDate: "date",
                Email: "text",
                DocumentName: "text",
                DocumentNumber: "text",
                DocumentSeries: "text",
                DocumentIssueOrganization: "text",
                DocumentIssueDate: "date",
            },
            currentValues: selCard,
            syncVal: this.props.updateSetValCallback,
        });
    }
}
