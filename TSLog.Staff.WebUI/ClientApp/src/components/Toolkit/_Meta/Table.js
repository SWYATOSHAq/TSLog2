import React, { Component } from 'react';

export class Table extends Component {
    static displayName = Table.name;

    constructor(props) {
        super(props);
        this.state = { headers: ["alfa", "beta", "gamma", "delta"], body: [["alfa1", "beta1", "gamma1", "delta1"], ["alfa2", "beta2", "gamma2", "delta2"]] };
    }

    static renderTable(headers, body) {
        console.log(headers);
        console.log(body);
        return (
            <table className='table table-striped' aria-labelledby="tabelLabel">
                <thead>
                    <tr>
                        {
                            headers.map((curRow, i) => {
                                return (<th key={i}>{curRow}</th>)
                            })
                        }
                    </tr>
                </thead>
                <tbody>
                    {body.map((row, i) => {
                        return (<tr key={i}>
                            {row.map((cell, j) => {
                                return (<td key={j}>{cell}</td>)
                            })}
                        </tr>)
                    })}
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
