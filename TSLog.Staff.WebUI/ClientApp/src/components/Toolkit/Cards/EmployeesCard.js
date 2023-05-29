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
                WorkMode: "Режим работы",
                BirthDate: "Дата рождения",
                Nationality: "Национальность",
                PhoneNumber: "Телефон",
                Email: "Почта",
                WMSlogin: "WMS логин",
                ADlogin: "AD логин",
                EmploymentDate: "Дата найма",
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
                WorkMode: "workMode",
                BirthDate: "date",
                PhoneNumber: "tel",
                Email: "email",
                WMSlogin: "WMS логин",
                ADlogin: "AD логин",
                EmploymentDate: "date",
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
