import React, { Component } from "react";
import "../../../scss/Arrow.scss";

export class Multiselect extends Component {
    static displayName = Multiselect.name;

    constructor(props) {
        super(props);
        let storedItems = this.props.DataValues;
        if (storedItems === undefined)
            storedItems = [];

        let inputItems = this.props.DataList;
        if (inputItems === undefined)
            inputItems = [];

        inputItems = inputItems.filter(item => !storedItems.includes(item));

        this.state = {
            storedItems,
            storedValue: [],
            inputItems,
            inputValue: [],
        };
    }

    handleChange(fieldType, event) {
        let newVal = event.target.value;
        let stateVal = this.state[fieldType];

        stateVal.indexOf(newVal) === -1
            ? stateVal.push(newVal)
            : stateVal.length === 1
                ? (stateVal = [])
                : stateVal.splice(stateVal.indexOf(newVal), 1);

        this.setState({ fieldType: stateVal });

        let callbackFunc = this.props.callbackOnChange;
        if (callbackFunc !== undefined)
            callbackFunc(stateVal.map(i => this.state.storedItems[i]));
    }

    setStoredItems() {
        let stateVal = this.state.inputValue;
        let dataArr = this.state.inputItems;
        let targetArr = this.state.storedItems;

        let { toList, fromlist, maskList } = this.moveData(dataArr, targetArr, stateVal);

        this.setState({
            inputValue: maskList,
            inputItems: fromlist,
            storedItems: toList,
        });
    }

    freeStoredItems() {
        let stateVal = this.state.storedValue;
        let dataArr = this.state.storedItems;
        let targetArr = this.state.inputItems;

        let { toList, fromlist, maskList } = this.moveData(dataArr, targetArr, stateVal);

        this.setState({
            storedValue: maskList,
            storedItems: fromlist,
            inputItems: toList,
        });
    }

    moveData(fromlist, toList, maskList) {
        let maskedItems = maskList.map(i => fromlist[i]);

        toList = toList.concat(maskedItems).sort();;
        fromlist = fromlist.filter(item => !maskedItems.includes(item)).sort();
        maskList = [];

        return { toList, fromlist, maskList };
    }

    render() {
        return (
            <div className="p-2 row">
                <div className="p-2 col-4">
                    <h5>Выбранные</h5>
                    <select
                        className="form-select"
                        multiple
                        value={this.state.storedValue}
                        onChange={this.handleChange.bind(this, 'storedValue')}
                        key={this.state.storedValue}
                    >
                        {
                            this.state.storedItems.map((row, i) => {
                                return (<option key={i} value={i}>{row.DisplayName}</option>)
                            })
                        }
                    </select>
                </div>
                <div className="col-4">
                    <div className="arrow-container">
                        <button className="arrow left" onClick={this.setStoredItems.bind(this)}></button>
                        <button className="arrow right" onClick={this.freeStoredItems.bind(this)}></button>
                    </div>
                </div>
                <div className="p-2 col-4">
                    <h5>Все</h5>
                    <select
                        className="form-select"
                        multiple
                        value={this.state.inputValue}
                        onChange={this.handleChange.bind(this, 'inputValue')}
                        key={this.state.inputValue}
                    >
                        {
                            this.state.inputItems.map((row, i) => {
                                return (<option key={i} value={i}>{row.DisplayName}</option>)
                            })
                        }
                    </select>
                </div>
            </div>

        );
    }
}