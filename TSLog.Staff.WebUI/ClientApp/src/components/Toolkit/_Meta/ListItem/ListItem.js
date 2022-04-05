import React, { Component } from 'react';
import './ListItem.css'

export class ListItem extends Component {
    static displayName = ListItem.name;

    state = {
        done: false,
        important: false
    }
    onLabelClick = () => {
        this.setState(({done}) => {
            return {
                done: !done
            }
        })
    }
    onMarkImportant = () => {
        this.setState((state) => {
            return {
                important: !state.important
            }
        })
    }
    render() {
        const { done, important } = this.state;
        let classNames = 'profession';
        if (done) {
            classNames += ' done';
        }
        if (important) {
            classNames += ' important';
        }
        return (
            <div className='change-button'>
                <span className= { classNames } onClick={ this.onLabelClick } >{this.props.label}</span>
                <div>
                    <button type="button" className="btn btn-outline-primary button-icons"
                    onClick={this.onMarkImportant}><i className="fa-solid fa-id-card-clip"></i></button>
                    <button type="button" className="btn btn-outline-danger button-icons"
                    onClick={this.props.onDeleted}><i className='fa fa-trash-o'/></button>
                </div>
            </div>
    );
    };
};
