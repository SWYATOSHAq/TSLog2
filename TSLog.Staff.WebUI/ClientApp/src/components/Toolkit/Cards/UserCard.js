import React, { Component } from 'react';

export class UserCard extends Component {
    static displayName = UserCard.name;

    constructor(props) {
        super(props);
        this.state = { currentCount: 0 };
        this.incrementCounter = this.incrementCounter.bind(this);
    }

    incrementCounter() {
        this.setState({
            currentCount: this.state.currentCount + 1
        });
    }

    render() {
        return (
            <div>
                <h1>Счетчик</h1>

                <p aria-live="polite">Текущее зничение: <strong>{this.state.currentCount}</strong></p>

                <button className="btn btn-primary" onClick={this.incrementCounter}>Увеличить</button>
            </div>
        );
    }
}
