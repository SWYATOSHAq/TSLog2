import { Table } from '../_Meta/Table';

export class PositionTable extends Table {
    static displayName = PositionTable.name;

    constructor(props) {
        super(props);
    }

    componentDidMount() {
        this.LoadData();
    }

    render() {
        let table = super.render();

        return (
            <div>
                <h1 id="tabelLabel" >Должности</h1>
                {table}
            </div>
        );
    }

    async LoadData() {
        const response = await fetch('/api/v1/Staff/Positions');
        const data = await response.json();
        console.log(data);

        super.setState({
            headers: { Name: "Наименование"},
            dataTypes: { Name: "string" },
            body: data
        });

    }
}
