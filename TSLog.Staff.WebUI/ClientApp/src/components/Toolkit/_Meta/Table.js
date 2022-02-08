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
            selectRow: (f) => { console.log("qwe " + f) },
        };
    }

    static renderTable(headers, dataTypes, body, changeCallback) {
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel" key={this.state === undefined}>

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
                                <tr key={i}>
                                    {
                                        Object.keys(headers).map((curRowKey, j) => {
                                            var curDataType = dataTypes[curRowKey];
                                            var cellData = curRow[curRowKey];

                                            if (curDataType === "datetime") {
                                                var parsedDate = new Date(cellData);
                                                cellData = parsedDate.toLocaleDateString() + " " + parsedDate.toLocaleTimeString();
                                            }

                                            if (curDataType === "date") {
                                                var parsedDate = new Date(cellData);
                                                cellData = parsedDate.toLocaleDateString();
                                            }

                                            if (curDataType === "time") {
                                                var parsedDate = new Date(cellData);
                                                cellData = parsedDate.toLocaleTimeString();
                                            }

                                            return (<td key={i + "-" + j}>{cellData}</td>)
                                        })
                                    }

                                    <td className="col-1">
                                        <ChangeButton
                                            SelectRow={changeCallback}
                                            rowIndex={i} />
                                    </td>
                                    <td className="col-1">
                                        <DeleteButton />
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
        let contents = Table.renderTable(this.state.headers, this.state.dataTypes, this.state.body, this.state.selectRow);
        return (
            <div>
                {contents}
            </div>
        );
    }
}
