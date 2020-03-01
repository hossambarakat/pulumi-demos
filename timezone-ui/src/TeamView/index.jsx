import React from "react";
import classNames from 'classnames';
import Timezone from './timezone';

class TimezoneList extends React.Component {

  render() {
    var containerClasses = classNames('timezone-list', {});

    return (
      <div className={containerClasses}>
        <div className="timezone-wrapper">
          {this.props.timezones.map(function(timezone) {
            return (
              <Timezone
                key={timezone.tz}
                timezone={timezone}
                time={this.props.time}
              />
            );
          }.bind(this))}
        </div>
        
      </div>
    );
  }

}

export default TimezoneList;