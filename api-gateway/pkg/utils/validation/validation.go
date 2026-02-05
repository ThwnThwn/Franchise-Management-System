package validation

import (
	"api-gateway/internal/domain/models/errormodel"
	"errors"
	"fmt"
	"net/http"
	"regexp"
	"strings"

	"github.com/gin-gonic/gin/binding"
	"github.com/go-playground/validator/v10"
)

func IsUserName(fl validator.FieldLevel) bool {
	s := fl.Field().String()

	reAllowed := regexp.MustCompile(`^[a-zA-Z0-9._]{8,20}$`)
	if !reAllowed.MatchString(s) {
		return false
	}

	reRepeat := regexp.MustCompile(`[_.]{2}`)
	if reRepeat.MatchString(s) {
		return false
	}

	if s[0] == '.' || s[0] == '_' || s[len(s)-1] == '.' || s[len(s)-1] == '_' {
		return false
	}

	return true
}

func NotEqualField(fl validator.FieldLevel) bool {
	field := fl.Field().String()
	other := fl.Parent().FieldByName(fl.Param()).String()
	return field != other
}

func init() {
	if v, ok := binding.Validator.Engine().(*validator.Validate); ok {
		// register for custom validation
		v.RegisterValidation("username", IsUserName)
		v.RegisterValidation("neqfield", NotEqualField)
	}
}

func TranslateValidationError(err error) *errormodel.BackendError {
	var ve validator.ValidationErrors
	if errors.As(err, &ve) {
		msgs := []string{}
		for _, fe := range ve {
			var msg string
			fieldName := fe.Field()
			paramName := fe.Param()
			switch fe.Tag() {
			case "required":
				msg = fmt.Sprintf("Vui lòng nhập %s", fieldName)
			case "email":
				msg = "Định dạng email không hợp lệ"
			case "username":
				msg = "Username phải dài 8-20 kí tự, chỉ chứa chữ, số, dấu chấm (.), và dấu gạch dưới (_)"
				// msg = fmt.Sprintf("%s must be 8–20 characters long and only contain letters, digits, dot (.), and underscore (_)", fieldName)
			case "eqfield":
				msg = fmt.Sprintf("%s phải giống với %s", fieldName, paramName)
			case "nefield":
				msg = fmt.Sprintf("%s phải khác với %s", fieldName, paramName)
			case "len":
				msg = fmt.Sprintf("%s phải dài %s kí tự", fieldName, paramName)
			case "gt":
				msg = fmt.Sprintf("%s phải lớn hơn %s", fieldName, paramName)
			case "min":
				msg = fmt.Sprintf("%s phải dài ít nhất %s kí tự", fieldName, paramName)
			case "max":
				if fe.Kind().String() == "int32" || fe.Kind().String() == "int" {
					msg = fmt.Sprintf("%s không được lớn hơn %s", fieldName, paramName)
				} else {
					msg = fmt.Sprintf("%s không được vượt quá %s kí tự", fieldName, paramName)
				}
			default:
				msg = fmt.Sprintf("%s: %s không hợp lệ", fieldName, fe.Tag())
			}
			msgs = append(msgs, msg)
		}
		return &errormodel.BackendError{
			Status: http.StatusBadRequest,
			Title:  "Yêu cầu không hợp lệ",
			Detail: strings.Join(msgs, "; "),
		}
	}

	if strings.Contains(err.Error(), "unmarshal") {
		return &errormodel.BackendError{
			Status: http.StatusBadRequest,
			Title:  "Dữ liệu không đúng định dạng",
			Detail: "Giá trị số quá lớn hoặc kiểu dữ liệu không hợp lệ",
		}
	}

	return &errormodel.BackendError{
		Status: http.StatusBadRequest,
		Title:  "Yêu cầu không hợp lệ",
		Detail: err.Error(),
	}
}
