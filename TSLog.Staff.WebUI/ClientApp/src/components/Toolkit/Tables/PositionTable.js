import { Table } from '../_Meta/Table';

export class PositionTable extends Table {
    static displayName = PositionTable.name;

    constructor(props) {
        super(props);
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
}
