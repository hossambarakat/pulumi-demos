import React from 'react'
import moment from 'moment-timezone';
class Timezone extends React.Component {


  render() {
    // We clone the time object itself so the this time is bound to
    // the global app time
    var localTime   = moment( this.props.time ).tz( this.props.timezone.tz );
    var fmtString = 'hh:mma z'
    var displayTime = localTime ? localTime.format(fmtString) : 'Unknown';
    var offset      = localTime ? localTime.format('Z') : '??:??';
    var hour        = localTime ? localTime.hour() : 'unknown';

    var timezoneClasses = 'timezone timezone-hour-' + hour;

    return (
      <div className={timezoneClasses}>
        <div className="timezone-header">
          <h3 className="timezone-time">{displayTime}</h3>
          <p className="timezone-offset">{offset}</p>
        </div>
        <div className="timezone-people">
        {this.props.timezone.people.map(function(person, idx){
            return (
              <div className="timezone-people-column" key={"column-" + idx}>
                <div>{person.name} - {person.location}</div>
              </div>
            );
          }.bind(this))}
        </div>
      </div>
    );
  }
}

export default Timezone;