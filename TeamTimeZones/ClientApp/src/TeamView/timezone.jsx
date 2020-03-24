import React from 'react'
import moment from 'moment-timezone';
import IconButton from '@material-ui/core/IconButton';
import DeleteIcon from '@material-ui/icons/Delete';


const Timezone = ({ timezone, time, onDelete }) => {
  var localTime = moment(time).tz(timezone.timeZone);
  var displayTime = localTime ? localTime.format('hh:mma z') : 'Unknown';
  var offset = localTime ? localTime.format('Z') : '??:??';

  return (
    <div>
      <div >
        <h3>{displayTime}</h3>
        <p>{offset}</p>
      </div>
      <div>
        {timezone.people.map(function (person, idx) {
          return (
            <div key={"column-" + idx}>
              <div>{person.name} - {person.country}
                <IconButton aria-label="delete"
                  onClick={() => onDelete(person.id)}
                >
                  <DeleteIcon />
                </IconButton>
              </div>

            </div>
          );
        })}
      </div>
    </div>
  );
}

export default Timezone;