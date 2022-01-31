import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { Counter } from './components/Counter';

import { UserTable } from './components/Toolkit/Tables/UserTable';

import './custom.css'

export default class App extends Component {
    static displayName = App.name;

    render() {
        return (
            <Layout>
                <Route exact path='/' component={UserTable} />
                <Route path='/counter' component={Counter} />
                <Route path='/fetch-data' component={FetchData} />

                <Route path='/user-table' component={UserTable} />
            </Layout>
        );
    }
}
