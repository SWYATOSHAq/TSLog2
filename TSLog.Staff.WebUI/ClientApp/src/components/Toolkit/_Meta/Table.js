import React, { Component } from 'react';

export class Table extends Component {
    static displayName = Table.name;

    constructor(props) {
        super(props);
        this.state = { headers: [], body: [[], []] };
    }

    static renderTable(headers, body) {
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
                                            return (<td>{curRow[curRowKey]}</td>)
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
        let contents = Table.renderTable(this.state.headers, this.state.body);
        return (
            <div>
                {contents}
            </div>
        );
    }
}
