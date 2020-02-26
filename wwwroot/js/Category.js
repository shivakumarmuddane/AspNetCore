var dataTable;

$(document).ready(function () {
    loadList();

})

function loadList() {
    dataTable = $('#DT_load').DataTable({
        "ajax": {
            "url": "/api/category",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "name", "width": "40%" },
            { "data": "displayOrder", "width": "30%" },      
            {
                "data": "id",
             "render": function (data) { 
                    return ` <div class="text-center">
                                       <a href="/Admin/Category/Upsert?Id=${data}" class="btn btn-success text-white editClick" style="cursor:pointer; width:100px;">
                                           <i class="far fa-edit"></i> Edit
                                       </a>
                                       <a  class="btn btn-danger text-white" style="cursor:pointer; width:100px;" onclick=Delete('/api/Category/Delete?Id=${data}')>
                                           <i class="far fa-trash-alt"></i> Delete
                                        </a>
                               </div>`;                   
                }, "width": "30%"
            }
        ],
        "language": {
            "emptyTable": "No Data Found."
        },
        "width": "100%",
        "order": [[1, "asc"]]
    });
}



function Delete(url) {
    swal({
        title: "Are you sure want to Delete?",
        text: " You will not be able to restore the data",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: 'DELETE',
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        dataTable.ajax.reload();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}