import React, { Component } from 'react';

export class Table extends Component {
    static displayName = Table.name;

    constructor(props) {
        super(props);
        this.state = { headers: [], dataTypes: [], body: [[], []] };
    }

    static renderTable(headers, dataTypes, body) {
        console.log(headers);
        console.log(body);
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        {
                            Object.keys(headers).map((curRowKey, i) => {
                                return (<th key={i}>{headers[curRowKey]}</th>)
                            })
                        }
                    </tr>
                </thead>
                <tbody>

                    {
                        body.map((curRow, i) => {
                            return (
                                <tr key={i}>
                                    {
                                        Object.keys(headers).map((curRowKey, i) => {
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

                                            return (<td>{cellData}</td>)
                                        })
                                    }
                                </tr>
                            )
                        })
                    }

                </tbody>
            </table>
        );
    }

    render() {
        let contents = Table.renderTable(this.state.headers, this.state.dataTypes, this.state.body);
        return (
            <div>
                {contents}
            </div>
        );
    }
}
