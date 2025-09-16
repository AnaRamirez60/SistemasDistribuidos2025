import datetime
from spyne import Fault

# Valida que el título no esté vacío
def validate_title(title):
    if not title or len(title.strip()) == 0:
        raise Fault(faultcode='Client', faultstring='El título es obligatorio.')
    return title

# Valida que la fecha no haya pasado
def validate_end_date(end_date):
    if end_date < datetime.date.today():
        raise Fault(faultcode='Client', faultstring='La fecha de finalización no puede ser en el pasado.')
    return end_date

# Valida que el id de la tarea sea mayor a 0
def validate_task_id(task_id):
    if task_id <= 0:
        raise Fault(faultcode='Client', faultstring='El ID debe ser un número positivo.')
    return task_id

# Valida que la tarea exista
def validate_task_exists(task):
    if not task:
        raise Fault(faultcode='Client.ResourceNotFound', faultstring='Tarea no encontrada.')
    return task