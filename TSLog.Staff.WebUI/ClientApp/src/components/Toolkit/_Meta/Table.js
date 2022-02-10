import React, { Component } from 'react';
import { ChangeButton } from '../_Meta/ChangeButton';
import { DeleteButton } from '../_Meta/DeleteButton';

export class Table extends Component {
    static displayName = Table.name;

    constructor(props) {
        super(props);
        this.state = {
            headers: {},
            dataTypes: {},
            body: [[], []],
            activeRowIndex: 0,
            selectRow: (f) => {  },
            deleteRow: (f) => {  },
        };
    }

    static renderTable(headers, dataTypes, body, changeCallback, deleteCallback, activeRowIndex) {
        return (
            <table className='table table-hover' aria-labelledby="tabelLabel" key={this.state === undefined}>

                <thead>
                    <tr>
                        {
                            Object.keys(headers).map((curRowKey, i) => {
                                return (<th key={i}>{headers[curRowKey]}</th>)
                            })
                        }

                        <th className="col-1"></th>
                        <th className="col-1"></th>
                    </tr>
                </thead>
                <tbody>

                    {
                        body.map((curRow, i) => {
                            return (
                                <tr key={i} className={activeRowIndex === i ? "bg-light" : "" }>
                                    {
                                        Object.keys(headers).map((curRowKey, j) => {
                                            var curDataType = dataTypes[curRowKey];
                                            var cellData = curRow[curRowKey];
                                            var parsedDate = null;

                                            if (curDataType === "datetime") {
                                                parsedDate = new Date(cellData);
                                                cellData = parsedDate.toLocaleDateString() + " " + parsedDate.toLocaleTimeString();
                                            }

                                            if (curDataType === "date") {
                                                parsedDate = new Date(cellData);
                                                cellData = parsedDate.toLocaleDateString();
                                            }

                                            if (curDataType === "time") {
                                                parsedDate = new Date(cellData);
                                                cellData = parsedDate.toLocaleTimeString();
                                            }

                                            if (curDataType === "bool") {
                                                cellData = cellData ? "Да" : "Нет";
                                            }

                                            if (curDataType === "company") {
                                                cellData = cellData.DisplayName;
                                            }
                                            return (<td key={i + "-" + j}>{cellData}</td>)
                                        })
                                    }

                                    <td className="col-1">
                                        <ChangeButton
                                            SelectRow={changeCallback}
                                            rowIndex={i}
                                        />
                                    </td>
                                    <td className="col-1">
                                        <DeleteButton
                                            DeleteRow={deleteCallback}
                                            rowIndex={i}
                                        />
                                    </td>
                                </tr>
                                )
                        })
                    }

                </tbody>
            </table>
        );
    }

    render() {
        //this.state.selectRow("--");
        let contents = Table.renderTable(this.state.headers, this.state.dataTypes, this.state.body, this.state.selectRow, this.state.deleteRow, this.state.activeRowIndex);
        return (
            <div>
                {contents}
            </div>
        );
    }
}
