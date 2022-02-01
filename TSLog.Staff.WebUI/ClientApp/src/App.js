import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';

import { DepartmentTable } from './components/Toolkit/Tables/DepartmentTable';
import { PositionTable } from './components/Toolkit/Tables/PositionTable';
import { ShiftTable } from './components/Toolkit/Tables/ShiftTable';
import { UserTable } from './components/Toolkit/Tables/UserTable';

import './custom.css'

export default class App extends Component {
    static displayName = App.name;

    render() {
        return (
            <Layout>
                <Route exact path='/' component={ShiftTable} />
                <Route path='/users' component={UserTable} />
                <Route path='/positions' component={PositionTable} />
                <Route path='/departments' component={DepartmentTable} />
            </Layout>
        );
    }
}
