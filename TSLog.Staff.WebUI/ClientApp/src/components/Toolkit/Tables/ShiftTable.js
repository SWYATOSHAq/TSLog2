import React, { Component } from 'react';
import { Table } from '../_Meta/Table';

export class ShiftTable extends Table {
    static displayName = ShiftTable.name;

    constructor(props) {
        super(props);
    }

    render() {
        let table = super.render();

        return (
            <div>
                <h1 id="tabelLabel" >bruh</h1>
                {table}
            </div>
        );
    }
}
